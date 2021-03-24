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
           lenAuroraCmd=8;
           targetAddress=address;
           myDev = BusFactory.CreateDev485(baudRate,parity, dataBits ,stopBits, COMorIP, port, lenSndBuffer, lenRecBuffer);
       }


      //public byte[] QueryDevice( byte[] frame)
      //{
      //   byte[] _buff=  myDev.QueryDevice(frame);
      //   return _buff;
      //}
      private byte[] _BuildFrame(byte[] AuroraCmd)
      {
        
        byte[] CRC = new byte[2];
        
        byte[] frame = new byte[lenAuroraCmd+2];
        // comando
        CRC=myDev.CCITT_CRC16(AuroraCmd);
        Buffer.BlockCopy(AuroraCmd,0,frame,0,AuroraCmd.Length);
               
        Buffer.BlockCopy(CRC,0,frame,AuroraCmd.Length,CRC.Length);
        return frame;    
                  
      }

      

       public Single CumulateTotalEnergy(bool Global=false)
      {
        byte[] rawValue = new byte[4];
        byte _global=0;
        if (Global==true) _global=1;
        Single value;

        byte[] cmd = new byte[lenAuroraCmd];
        cmd[0]= targetAddress;
        cmd[1]= 68; // comando per leggere DSP
        cmd[2]= 6;   // 

        cmd[5]= _global;

        byte[] frame = _BuildFrame(cmd);
        
        byte[] answer = myDev.QueryDevice(frame); 
        
        value= myDev.BytesToSingle(answer,3);
        return value;

      }

        public Single CumulateCurrentDayEnergy(bool Global=false)
      {
        byte[] rawValue = new byte[4];
        byte _global=0;
        if (Global==true) _global=1;
        Single value;

        byte[] cmd = new byte[lenAuroraCmd];
        cmd[0]= targetAddress;
        cmd[1]= 68; // comando per leggere DSP
        cmd[2]= 1;   // 

        cmd[5]= _global;

        byte[] frame = _BuildFrame(cmd);
        
        byte[] answer = myDev.QueryDevice(frame); 
        
        value= myDev.BytesToSingle(answer,3);
        return value;

      }


      
      public Single ReadGridCurrent(bool Global=false)
      {
        byte[] rawValue = new byte[4];
        byte _global=0;
        if (Global==true) _global=1;
        Single value;

        byte[] cmd = new byte[lenAuroraCmd];
        cmd[0]= targetAddress;
        cmd[1]= 59; // comando per leggere DSP
        cmd[2]= 2;   // Tipo di valore letto dal DSP. 
        cmd[3]= _global;

        byte[] frame = _BuildFrame(cmd);
        
        byte[] answer = myDev.QueryDevice(frame); 
        
        value= myDev.BytesToSingle(answer,3);
        return value;
      }
      public Single ReadGridPower(bool Global=false)
      {
        byte[] rawValue = new byte[4];
        byte _global=0;
        if (Global==true) _global=1;
        Single value;

        byte[] cmd = new byte[lenAuroraCmd];
        cmd[0]= targetAddress;
        cmd[1]= 59; // comando per leggere DSP
        cmd[2]= 3;   // Tipo di valore letto dal DSP. 
        cmd[3]= _global;

        byte[] frame = _BuildFrame(cmd);
        
        byte[] answer = myDev.QueryDevice(frame); 
        
        value= myDev.BytesToSingle(answer,3);
        return value;
      }


      public Single ReadGridVoltage(bool Global=false)
      {
        byte[] rawValue = new byte[4];
        byte _global=0;
        if (Global==true) _global=1;
        Single value;

        byte[] cmd = new byte[lenAuroraCmd];
        cmd[0]= targetAddress;
        cmd[1]= 59; // comando per leggere DSP
        cmd[2]= 1;   // Tipo di valore letto dal DSP. 1=GridVoltage
        cmd[3]= _global;

        byte[] frame = _BuildFrame(cmd);
        
        byte[] answer = myDev.QueryDevice(frame); 
        
        value= myDev.BytesToSingle(answer,3);
        return value;
      }

      public Single ReadFrequency(bool Global=false)
      {
        byte[] rawValue = new byte[4];
        byte _global=0;
        if (Global==true) _global=1;
        Single value;

        byte[] cmd = new byte[lenAuroraCmd];
        cmd[0]= targetAddress;
        cmd[1]= 59; // comando per leggere DSP
        cmd[2]= 4;   // Tipo di valore letto dal DSP
        cmd[3]= _global;

        byte[] frame = _BuildFrame(cmd);
        
        byte[] answer = myDev.QueryDevice(frame); 
        
        value= myDev.BytesToSingle(answer,3);
        return value;
      }



     



    }

}