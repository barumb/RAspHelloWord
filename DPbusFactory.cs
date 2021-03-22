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

   public abstract class DPbusFactory
   {
       public abstract DPdev485 CreateDev485(int baudRate,  Parity parity, int dataBits, StopBits stopBits, string COMorIP="", int port=0,int lenSndBuffer=32,int lenRecBuffer=32);
      
   }
}