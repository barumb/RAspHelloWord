using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;
using RaspHelloWord;
using System.IO.Ports;

namespace RaspHelloWord
{
    public class DPAuroraInverter
    {
       private DPdev485 myDev;
       private byte targetAddress;
       private int lenAuroraCmd; //lunghezza comando escluso CRC
       public DPAuroraInverter(DPbusFactory BusFactory, byte address, int baudRate,  Parity parity, int dataBits, StopBits stopBits, string COMorIP="", int port=0,int lenSndBuffer=32,int lenRecBuffer=32 )
       {
           lenAuroraCmd=8
           targetAddress=address;
           myDev = BusFactory.CreateDev485(baudRate,parity, dataBits ,stopBits, COMorIP, port, lenSndBuffer, lenRecBuffer);
       }


      public byte[] QueryDevice( byte[] frame)
      {
         byte[] _buff=  myDev.QueryDevice(frame);
         return _buff;
      }

      public Single ReadPower()
      {
        Single value;
        byte[] rawValue = new byte[4];
        byte[] CRC = new byte[2];
        byte[] msg = new byte[lenAuroraCmd];
        byte[] frame = new byte[lenAuroraCmd+2];
        // comando
        msg[0] = targetAddress;
        msg[1] = ;// comando

        Buffer.BlockCopy(msg,0,frame,0,msg.Length);
        Buffer.BlockCopy(CRC,0,frame,msg.Length,CRC.Length);
        
        byte[] answer = myDev.QueryDevice(frame); 
        
        value=BitConverter.ToSingle(answer,1);  // 
        return value;
      }




    }

}