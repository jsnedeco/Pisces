using System;
using System.Collections.Generic;
using System.IO;
using Pisces.IO.Interfaces;

namespace Pisces.IO
{
    public abstract class BaseVcfFileWriter<T> : IVcfFileWriter<T>
    {
        protected string OutputFilePath;
        protected StreamWriter Writer;
        protected int BufferLimit;
        protected List<T> BufferList;
        protected bool AllowMultipleVcfLinesPerLoci = true;

        protected BaseVcfFileWriter(string outputFilePath, int bufferLimit)
        {
            OutputFilePath = outputFilePath;

            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(outputFilePath)))
                {
                    try
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath));
                    }
                    catch (Exception)
                    {
                        throw new ArgumentException(string.Format("Failed to create the Output Folder: {0}", outputFilePath));
                    }
                }
                File.Delete(OutputFilePath);
                Writer = new StreamWriter(new FileStream(outputFilePath, FileMode.Create));
                Writer.NewLine = "\n";
                BufferLimit = bufferLimit;
                BufferList = new List<T>(BufferLimit);
            }
            catch (Exception ex)
            {
                throw new IOException(String.Format("Failed to create {0} in the specified folder. Ex {1}", outputFilePath, ex));
            }

        }

        public abstract void WriteHeader();

        public void Dispose()
        {
            if (Writer != null)
            {
                FlushBuffer();
                Writer.Dispose();
                Writer = null;
            }
        }

        public void FlushBuffer(IRegionMapper mapper = null)
        {

            if (AllowMultipleVcfLinesPerLoci)
            {
                //we dont crush
                foreach (var variant in BufferList)
                {
                    WriteSingleAllele(Writer, variant, mapper);
                }
            }
            else //the VCF spec that makes parsers cry
            {
                GroupsAllelesThenWrite(Writer, BufferList, mapper);           
            }

            BufferList.Clear();
        }

        protected abstract void GroupsAllelesThenWrite(StreamWriter writer, List<T> variants, IRegionMapper mapper = null);
        protected abstract void WriteSingleAllele(StreamWriter writer, T variant, IRegionMapper mapper = null);

        public virtual void WriteRemaining(IRegionMapper mapper = null)
        {
            // do nothing by default
        }

        public virtual void Write(string msg, IRegionMapper mapper = null)
        {
            if (Writer == null)
                throw new IOException("Stream already closed");

            FlushBuffer(mapper);

            Writer.WriteLine(msg);

            FlushBuffer(mapper);
        }

        public virtual void Write(List<string> msgs, IRegionMapper mapper = null)
        {
            if (Writer == null)
                throw new IOException("Stream already closed");

            FlushBuffer(mapper);

            foreach (var msg in msgs)
                Writer.WriteLine(msg);

            FlushBuffer(mapper);
        }


        public virtual void Write(IEnumerable<T> BaseCalledAlleles, IRegionMapper mapper = null)
        {
            if (Writer == null)
                throw new IOException("Stream already closed");

            BufferList.AddRange(BaseCalledAlleles);

            if (BufferList.Count >= BufferLimit)
                FlushBuffer(mapper);
        }

        protected void OnException(Exception ex)
        {
            BufferList.Clear(); // dont care about list, clear now so we dont try to flush again later

            Dispose();
            File.Delete(OutputFilePath);

            // throw again
            throw ex;
        }
    }
}