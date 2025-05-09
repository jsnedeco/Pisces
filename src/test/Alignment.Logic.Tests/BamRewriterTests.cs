using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using Alignment.Domain;
using Alignment.IO;
using Moq;
using Alignment.IO.Sequencing;
using Alignment.Domain.Sequencing;
using Xunit;

namespace Alignment.Logic.Tests
{
    public class BamRewriterTests
    {
        [Fact]
        public void ExecuteTest()
        {

            var alignmentsToRead = new List<BamAlignment>()
            {
                CreateAlignment("pair"),
                CreateAlignment("pair2"),
                CreateAlignment("single")
            };
            var writtenAlignments = new List<BamAlignment>();
            var unpairedAlignments = new List<BamAlignment>();

            var bamWriter = new Mock<IBamWriterMultithreaded>();
            bamWriter.Setup(x => x.WriteAlignment(It.IsAny<BamAlignment>())).Callback<BamAlignment>((b) => writtenAlignments.Add(b));

            var bamWriterHandle = new Mock<IBamWriterHandle>();
            var bamWriterHandleList = new List<IBamWriterHandle>();
            bamWriterHandleList.Add(bamWriterHandle.Object);

            bamWriterHandle.Setup(x => x.WriteAlignment(It.IsAny<BamAlignment>())).Callback<BamAlignment>((b) => writtenAlignments.Add(b));

            bamWriter.Setup(x => x.GenerateHandles()).Returns(() => { return bamWriterHandleList; });
            bamWriter.Setup(x => x.Flush());

            var alignmentPairFilter = new Mock<IAlignmentPairFilter>();
            alignmentPairFilter.Setup(f => f.TryPair(It.IsAny<BamAlignment>(), It.IsAny<PairStatus>()))
                .Returns<BamAlignment, PairStatus>((b, p) =>
                {
                    if (b.Name.StartsWith("pair"))
                    {
                        var x = new ReadPair(b);
                        x.AddAlignment(b);
                        return x;
                    }
                    unpairedAlignments.Add(b);
                    return null;
                }
                );

            alignmentPairFilter.Setup(f => f.GetFlushableUnpairedReads()).Returns(unpairedAlignments);

            BlockingCollection<Task> taskQueue = new BlockingCollection<Task>();

            // Create a thread pool with 1 thread. It will execute any tasks
            // added to the taskQueue by the BamRewriter.
            ThreadPool threadPool = new ThreadPool(taskQueue, 1);

            var readPairHandler = new Mock<IReadPairHandler>();
            readPairHandler.Setup(h => h.ExtractReads(It.IsAny<ReadPair>())).Returns<ReadPair>(
                (p) =>
                {
                    var list = new List<BamAlignment>();
                    list.AddRange(p.Read1Alignments);
                    list.AddRange(p.Read2Alignments);
                    return list;
                }
                );

            // Given a list of reads, should try to pair them all
            // Should flush to bam once the buffer reaches the specified size
            // If specifying getUnpaired = true, should also flush unpaired reads (as designated by the filter) to the bam
            var bamRewriter = new BamRewriter(MockBamReader(alignmentsToRead), bamWriter.Object,
                alignmentPairFilter.Object, new List<IReadPairHandler> { readPairHandler.Object }, taskQueue, true);
            bamRewriter.Execute();
            Assert.Equal(5, writtenAlignments.Count);

            // Should get all of the reads flushed, regardless of buffer size
            writtenAlignments.Clear();
            unpairedAlignments.Clear();
            bamRewriter = new BamRewriter(MockBamReader(alignmentsToRead), bamWriter.Object,
                alignmentPairFilter.Object, new List<IReadPairHandler> { readPairHandler.Object }, taskQueue, true);
            bamRewriter.Execute();
            Assert.Equal(5, writtenAlignments.Count);

            writtenAlignments.Clear();
            unpairedAlignments.Clear();
            bamRewriter = new BamRewriter(MockBamReader(alignmentsToRead), bamWriter.Object,
                alignmentPairFilter.Object, new List<IReadPairHandler> { readPairHandler.Object }, taskQueue, true);
            bamRewriter.Execute();
            Assert.Equal(5, writtenAlignments.Count);

            // If getUnpaired = false, should not flush unpaired reads to the bam
            writtenAlignments.Clear();
            unpairedAlignments.Clear();
            bamRewriter = new BamRewriter(MockBamReader(alignmentsToRead), bamWriter.Object,
                alignmentPairFilter.Object, new List<IReadPairHandler> { readPairHandler.Object }, taskQueue, false);
            bamRewriter.Execute();
            Assert.Equal(4, writtenAlignments.Count);
            Assert.True(writtenAlignments.All(a => a.Name.StartsWith("pair")));
        }

        private IBamReader MockBamReader(List<BamAlignment> alignments)
        {
            return new MockBamReader(alignments);
        }

        private static BamAlignment CreateAlignment(string name)
        {
            return new BamAlignment()
            {
                Name = name,
                Qualities = new byte[0]
            };
        }
    }
}