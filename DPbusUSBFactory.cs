using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;
using RaspHelloWord;
using System.IO.Ports;

namespace RaspHelloWord
{
   public  class DPbusUSBFactory:DPbusFactory
   { 

       public override DPdev485 CreateDev485(int baudRate,  Parity parity, int dataBits, StopBits stopBits, string COMorIP="", int port=0, int lenSndBuffer=32,int lenRecBuffer=32)
       {
           DPdev485byUSB _cls = new DPdev485byUSB(baudRate,  parity, dataBits, stopBits, COMorIP, port, lenSndBuffer, lenRecBuffer);

           return _cls; 
       }
  

   }
}