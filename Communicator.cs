using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;
using RaspHelloWord;
//add
using System.IO.Ports;

namespace RaspHelloWord
{
    public class Communicator
    {
      private DPdev485 _target;

      public Communicator (DPbusFactory BusFactory, int baudRate,  Parity parity, int dataBits, StopBits stopBits, string COMorIP="", int port=0,int lenSndBuffer=32,int lenRecBuffer=32  )
      {
          _target = BusFactory.CreateDev485(baudRate,parity, dataBits ,stopBits, COMorIP, port, lenSndBuffer, lenRecBuffer);

      }

      public byte[] QueryDevice( byte[] frame)
      {
         byte[] _buff=  _target.QueryDevice(frame);
         return _buff;
      }
    }


}