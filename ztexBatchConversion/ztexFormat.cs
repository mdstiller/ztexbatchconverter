// Decompiled with JetBrains decompiler
// Type: Coba.ztexformat
// Assembly: Coba, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8C2EE1-5961-490D-A331-13E48358E488
// Assembly location: F:\Emulation\ROMs\3DS\3DS Rom Tool GUI 3.1 - voddy\fantasy-life\unpacked\romfs\Coba.exe

using System;
using System.IO;
using System.Text;

namespace Coba
{
    public class ztexformat
    {
        public ztexformat.ztexheader Header = new ztexformat.ztexheader();
        private string _path;
        private Stream _strm;
        public ztexformat.ztexentry[] Entry;
        private byte[] temp;

        public ztexformat(string path)
        {
            this._path = path;
            this._strm = (Stream)new FileStream(this._path, FileMode.Open);
            this.getHeaderData();
            this.Entry = new ztexformat.ztexentry[this.Header.Count];
            this.getTextureHeaderData();
            this._strm.Close();
        }

        public string EncodeText(params byte[] input)
        {
            if ((int)input[0] < 32)
                return "";
            return Encoding.ASCII.GetString(input);
        }

        public void getHeaderData()
        {
            this._strm.Position = 4L;
            this.temp = new byte[2]
      {
        (byte) this._strm.ReadByte(),
        (byte) this._strm.ReadByte()
      };
            this.Header.Count = (int)BitConverter.ToUInt16(this.temp, 0);
            this.temp = new byte[2]
      {
        (byte) this._strm.ReadByte(),
        (byte) this._strm.ReadByte()
      };
            this.Header.u1 = (int)BitConverter.ToUInt16(this.temp, 0);
            this.temp = new byte[2]
      {
        (byte) this._strm.ReadByte(),
        (byte) this._strm.ReadByte()
      };
            this.Header.u2 = (int)BitConverter.ToUInt16(this.temp, 0);
        }

        public void getTextureHeaderData()
    {
      this._strm.Position = 12L;
      for (int index1 = 0; index1 < this.Header.Count; ++index1)
      {
        this.Entry[index1].Name = "";
        for (int index2 = 0; index2 < 64; ++index2)
        {
          // ISSUE: explicit reference operation
          // ISSUE: explicit reference operation
          (this.Entry[index1]).Name += this.EncodeText((byte) this._strm.ReadByte());
        }
        this.Entry[index1].Checksum = new byte[4]
        {
          (byte) this._strm.ReadByte(),
          (byte) this._strm.ReadByte(),
          (byte) this._strm.ReadByte(),
          (byte) this._strm.ReadByte()
        };
        this.Entry[index1].Offset = BitConverter.ToUInt32(new byte[4]
        {
          (byte) this._strm.ReadByte(),
          (byte) this._strm.ReadByte(),
          (byte) this._strm.ReadByte(),
          (byte) this._strm.ReadByte()
        }, 0);
        this.Entry[index1].u1 = BitConverter.ToUInt32(new byte[4]
        {
          (byte) this._strm.ReadByte(),
          (byte) this._strm.ReadByte(),
          (byte) this._strm.ReadByte(),
          (byte) this._strm.ReadByte()
        }, 0);
        this.Entry[index1].Size = BitConverter.ToUInt32(new byte[4]
        {
          (byte) this._strm.ReadByte(),
          (byte) this._strm.ReadByte(),
          (byte) this._strm.ReadByte(),
          (byte) this._strm.ReadByte()
        }, 0);
        this.Entry[index1].W = (uint) BitConverter.ToUInt16(new byte[2]
        {
          (byte) this._strm.ReadByte(),
          (byte) this._strm.ReadByte()
        }, 0);
        this.Entry[index1].H = (uint) BitConverter.ToUInt16(new byte[2]
        {
          (byte) this._strm.ReadByte(),
          (byte) this._strm.ReadByte()
        }, 0);
        this.Entry[index1].u2 = (uint) this._strm.ReadByte();
        this.Entry[index1].TextureFormat = (uint) this._strm.ReadByte();
        this.Entry[index1].u4 = (uint) BitConverter.ToUInt16(new byte[2]
        {
          (byte) this._strm.ReadByte(),
          (byte) this._strm.ReadByte()
        }, 0);
      }
    }

        public struct ztexheader
        {
            public int Count;
            public int u1;
            public int u2;
        }

        public struct ztexentry
        {
            public string Name;
            public byte[] Checksum;
            public uint Offset;
            public uint u1;
            public uint Size;
            public uint W;
            public uint H;
            public uint u2;
            public uint TextureFormat;
            public uint u4;
        }
    }
}
