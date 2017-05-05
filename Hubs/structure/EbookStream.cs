using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;

namespace eDocumentReader.Hubs
{
    public class EbookStream : Stream
    {
        public static readonly long LENGTH_INC = 2000000000; //about 12 hours of audio length
        public static readonly long MIN_GAP = 163840;
        FileStream fs;
        TimeSpan waitTime = new TimeSpan(1); //cause the thread to sleep waitTime milliseconds while the queue is empty
        long rtLength = LENGTH_INC;
        long currentPos = 0;

        static byte[] leftOver;
 
        private ConcurrentQueue<byte[]> conQueue;
        private ArrayList byteList;

        private bool active;

        public EbookStream(ref ConcurrentQueue<byte[]> conQueue)
        {
            // TODO: Complete member initialization
            this.conQueue = conQueue;
            leftOver = new byte[0];
            byteList = new ArrayList();
        }
        public override void Flush()
        {
            //TODO: save the buffer to a file
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            long timeNow = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            int ret = getBlockData(ref buffer);
            currentPos += ret;
            if (currentPos + MIN_GAP > rtLength)
            {
                rtLength += LENGTH_INC;
            }
            long timeAfter = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            return ret;
        }
        private int getBlockData(ref byte[] buf)
        {
            byte[] block;
            long timeNow = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            while (byteList.Count < buf.Length && active)
            {
                while (!conQueue.TryDequeue(out block) && active)
                {
                    //the queue is empty, wait for data
                    Thread.Sleep(waitTime);
                }
                if (block != null)
                {
                    byteList.AddRange(block.ToList());
                }
            }
            int ret = buf.Length;
            if (byteList.Count >= buf.Length)
            {
                byteList.CopyTo(0, buf, 0, buf.Length);
                byteList.RemoveRange(0, buf.Length);
            }
            else
            {
                ret = byteList.Count;
                byteList.CopyTo(0, buf, 0, ret);
                byteList.Clear();
            }

            return ret;
        }

        public void enable(bool b)
        {
            active = b;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            Debug.WriteLine("--> seek() offset=" + offset + ", origin="+origin);
            return 0;
        }

        public override void SetLength(long value)
        {
            Debug.WriteLine("--> setLength() value=" + value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Debug.WriteLine("--> write() buffer size=" + buffer.Length + ", offset=" + offset + ", count=" + count);
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get {return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override long Length
        {
            //since we are working on ongoing real time audio, we don't 
            //know the Length of the stream. We can set the arbitrary length.
            //if the position is about to exceed the length, we can increase
            //the length
            get
            {
                return rtLength;
            }
        }

        public override long Position
        {
            // the current position of the stream since the audio start
            get
            {
                return currentPos;
            }
            set
            {
                //Seems we don't need to set the position here
            }
        }
    }
}