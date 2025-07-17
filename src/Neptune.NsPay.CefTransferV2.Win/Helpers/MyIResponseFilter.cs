using CefSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.CefTransferV2.Win.Helpers
{
    public class MyIResponseFilter : IResponseFilter
    {
        private MemoryStream _stream;

        public List<byte> DataAll = new List<byte>();
        public byte[] DataStream
        {
            get { return _stream.ToArray(); }
        }

        public FilterStatus Filter(System.IO.Stream dataIn, out long dataInRead, System.IO.Stream dataOut, out long dataOutWritten)
        {
            try
            {
                if (dataIn == null || dataIn.Length == 0)
                {
                    dataInRead = 0;
                    dataOutWritten = 0;

                    return FilterStatus.Done;
                }

                dataInRead = dataIn.Length;
                dataOutWritten = Math.Min(dataInRead, dataOut.Length);

                dataIn.CopyTo(dataOut);

                dataIn.Seek(0, SeekOrigin.Begin);
                byte[] bs = new byte[dataIn.Length];
                dataIn.Read(bs, 0, bs.Length);
                DataAll.AddRange(bs);

                dataInRead = dataIn.Length;
                dataOutWritten = dataIn.Length;

                //// Copy to stream
                //dataInRead = dataIn.Length;
                //dataOutWritten = Math.Min(dataInRead, dataOut.Length);

                ////Important we copy dataIn to dataOut
                //dataIn.CopyTo(dataOut);

                ////Copy data to stream
                //dataIn.Position = 0;
                //dataIn.CopyTo(_stream);

                return FilterStatus.NeedMoreData;
            }
            catch (Exception ex)
            {
                dataInRead = dataIn.Length;
                dataOutWritten = dataIn.Length;

                return FilterStatus.Done;
            }
        }

        public bool InitFilter()
        {
            //NOTE: We could initialize this earlier, just one possible use of InitFilter
            _stream = new MemoryStream();

            return true;
        }

        public void Dispose()
        {
            _stream.Dispose();
            _stream = null;
        }
    }
}
