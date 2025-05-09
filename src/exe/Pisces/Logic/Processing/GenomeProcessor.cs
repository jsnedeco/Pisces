﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pisces;
using Pisces.Domain.Interfaces;
using Pisces.Domain.Models;
using Pisces.IO;
using Pisces.Processing.Logic;

namespace CallVariants.Logic.Processing
{
    public class GenomeProcessor : BaseGenomeProcessor
    {
        private readonly Dictionary<string, VcfFileWriter> _writerByChrLookup = new Dictionary<string, VcfFileWriter>();
        private readonly Dictionary<string, StrandBiasFileWriter> _sbBiasWriterByChrLookup = new Dictionary<string, StrandBiasFileWriter>();
        private readonly Dictionary<string, AmpliconBiasFileWriter> _abBiasWriterByChrLookup = new Dictionary<string, AmpliconBiasFileWriter>();

        private readonly List<VcfFileWriter> _writers = new List<VcfFileWriter>(); 
        private readonly List<StrandBiasFileWriter> _sbBiasWriters = new List<StrandBiasFileWriter>();
        private readonly List<AmpliconBiasFileWriter> _abBiasWriters = new List<AmpliconBiasFileWriter>();

        private readonly Factory _factory;
        private readonly bool _writeHeader;

        public GenomeProcessor(Factory factory, IGenome genome, bool throttlePerBam = true, bool writeHeader = true)
            : base(factory.WorkRequests.Where(
                w => w.GenomeDirectory.Equals(genome.Directory, StringComparison.CurrentCultureIgnoreCase))
                .ToList(), genome, throttlePerBam)
        {
            _factory = factory;
            _writeHeader = writeHeader;
        }

        protected override void Initialize()
        {
            if (ShouldThrottle)
            {
                // one writer per bam
                foreach (var workRequest in WorkRequests)
                {
                    var vcfWriter = _factory.CreateVcfWriter(workRequest.OutputFilePath, new VcfWriterInputContext
                    {
                        ReferenceName = Genome.Directory,
                        QuotedCommandLineString = _factory.GetCommandLine(),
                        SampleName = Path.GetFileName(workRequest.BamFilePath),
                        ContigsByChr = Genome.ChromosomeLengths
                    });

                    if (_writeHeader)
                        vcfWriter.WriteHeader();


                    StrandBiasFileWriter sbBiasFileWriter = null;
                    AmpliconBiasFileWriter abBiasFileWriter = null;
                    if (_factory.Options.OutputBiasFiles)
                    {
                        sbBiasFileWriter = _factory.CreateStrandBiasFileWriter(workRequest.OutputFilePath);
                        abBiasFileWriter = _factory.CreateAmpliconBiasFileWriter(workRequest.OutputFilePath);
                    }

                    // use same writer across all chr.  we will never be writing multiple chrs for a given bam at the same time
                    foreach (var chrName in Genome.ChromosomesToProcess)
                    {
                        var writerKey = GetChrOutputPath(workRequest, chrName);

                        _writerByChrLookup[writerKey] = vcfWriter;
                        _sbBiasWriterByChrLookup[writerKey] = sbBiasFileWriter;
                        _abBiasWriterByChrLookup[writerKey] = abBiasFileWriter;
                    }

                    _writers.Add(vcfWriter);

                    if (sbBiasFileWriter != null)
                        _sbBiasWriters.Add(sbBiasFileWriter);

                    if (abBiasFileWriter != null)
                        _abBiasWriters.Add(abBiasFileWriter);
                }
            }
            else
            {
                // one writer per bam and per chr
                // files will get stitched together later
                foreach (var workRequest in WorkRequests)
                {
                    foreach (var chrName in Genome.ChromosomesToProcess)
                    {
                        var outputPath = GetChrOutputPath(workRequest, chrName);

                        var vcfWriter = _factory.CreateVcfWriter(outputPath, new VcfWriterInputContext
                        {
                            ReferenceName = Genome.Directory,
                            QuotedCommandLineString = _factory.GetCommandLine(),
                            SampleName = Path.GetFileName(workRequest.BamFilePath),
                            ContigsByChr = Genome.ChromosomeLengths
                        });

                        StrandBiasFileWriter sbBiasFileWriter = null;
                        AmpliconBiasFileWriter abBiasFileWriter = null;
                        if (_factory.Options.OutputBiasFiles)
                        {
                            sbBiasFileWriter = _factory.CreateStrandBiasFileWriter(outputPath);
                            abBiasFileWriter = _factory.CreateAmpliconBiasFileWriter(outputPath);
                        }

                        var writerKey = outputPath;

                        _writerByChrLookup[writerKey] = vcfWriter;
                        _sbBiasWriterByChrLookup[writerKey] = sbBiasFileWriter;
                        _abBiasWriterByChrLookup[writerKey] = abBiasFileWriter;

                        _writers.Add(vcfWriter);

                        if (sbBiasFileWriter != null)
                            _sbBiasWriters.Add(sbBiasFileWriter);

                        if (abBiasFileWriter != null)
                            _abBiasWriters.Add(abBiasFileWriter);
                    }
                }
            }
        }

        protected override void Finish()
        {
            foreach (var writer in _writers)
            {
                writer.Dispose();
            }

            foreach (var writer in _sbBiasWriters)
            {
                writer.Dispose();
            }

            foreach (var writer in _abBiasWriters)
            {
                writer.Dispose();
            }

            if (!ShouldThrottle)
                CombinePerChromosomeFiles();
        }

        protected override void Process(BamWorkRequest workRequest, ChrReference chrReference)
        {
            var writerKey = GetChrOutputPath(workRequest, chrReference.Name);

            var caller = _factory.CreateSomaticVariantCaller(chrReference, workRequest.BamFilePath,
                _writerByChrLookup[writerKey], _sbBiasWriterByChrLookup[writerKey], _abBiasWriterByChrLookup[writerKey], null, Genome.ChromosomesToProcess);
            
            caller.Execute();
        }

        private void CombinePerChromosomeFiles()
        {
            foreach (var workRequest in WorkRequests)
            {
                //TODO combine the bias output files
                using (var vcfWriter = _factory.CreateVcfWriter(workRequest.OutputFilePath, new VcfWriterInputContext
                {
                    ReferenceName = Genome.Directory,
                    QuotedCommandLineString = _factory.GetCommandLine(),
                    SampleName = Path.GetFileName(workRequest.BamFilePath),
                    ContigsByChr = Genome.ChromosomeLengths
                }))
                {
                    vcfWriter.WriteHeader();
                }

                using (var writer = new FileStream(workRequest.OutputFilePath, FileMode.Append, FileAccess.Write, FileShare.None))
                {
                    foreach (var chrName in Genome.ChromosomesToProcess)
                    {
                        var chrFilePath = GetChrOutputPath(workRequest, chrName);
                        using (var reader = File.OpenRead(chrFilePath))
                        {
                            reader.CopyTo(writer);
                        }

                        File.Delete(chrFilePath);
                    }
                }
            }
        }

        private string GetChrOutputPath(BamWorkRequest workRequest, string chrName)
        {
            return workRequest.OutputFilePath + "_" + chrName;
        }
    }
}
