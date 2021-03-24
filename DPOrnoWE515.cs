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
    public class DPOrnoWE515
    {
       private DPdev485 myDev;
       private byte targetAddress;
       private int lenAuroraCmd; //lunghezza comando escluso CRC
       public DPOrnoWE515(DPbusFactory BusFactory, byte address, int baudRate,  Parity parity, int dataBits, StopBits stopBits, string COMorIP="", int port=0,int lenSndBuffer=32,int lenRecBuffer=32 )
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
      private byte[] _BuildFrame(byte[] ModbusCmd)
      {
        
        byte[] CRC = new byte[2];
        
        byte[] frame = new byte[ModbusCmd.Length+2];
        // comando
        CRC= myDev.Modbus_Crc16(ModbusCmd);
        Buffer.BlockCopy(ModbusCmd,0,frame,0,ModbusCmd.Length);
               
        Buffer.BlockCopy(CRC,0,frame,ModbusCmd.Length,CRC.Length);
        return frame;    
                  
      }
      
      public Single ReadActivePower()
      {
        byte[] rawValue = new byte[4];
       
        Single value;

        byte[] cmd = new byte[6];
        cmd[0]= targetAddress;
        cmd[1]= 0x03; // Codice funzione modbus. 3=read 16bbit register
        cmd[2]= 0x01; // address MSB
        cmd[3]= 0x40; // address LSB
        cmd[4]= 0x0;  // MSB qty to read
        cmd[5]= 0x2;  // LSB qty to read

        byte[] frame = _BuildFrame(cmd);
        
        byte[] answer = myDev.QueryDevice(frame); 
        
        value= (myDev.BytesToUint32(answer,2))/1000;
        return value;
      }

      public Single ReadVoltage()
      {
 byte[] rawValue = new byte[4];
       
        Single value;

        byte[] cmd = new byte[6];
        cmd[0]= targetAddress;
        cmd[1]= 0x03; // Codice funzione modbus. 3=read 16bbit register
        cmd[2]= 0x01; // address MSB
        cmd[3]= 0x31; // address LSB
        cmd[4]= 0x0;  // MSB qty to read
        cmd[5]= 0x1;  // LSB qty to read

        byte[] frame = _BuildFrame(cmd);
        
        byte[] answer = myDev.QueryDevice(frame); 
        
        value= (myDev.BytesToSingle(answer,2))/100; // va scritto il metodo Byte to Uint
        return value;

      }

      public Single ReadCurrent()
      {
        byte[] rawValue = new byte[4];
       
        Single value;

        byte[] cmd = new byte[6];
        cmd[0]= targetAddress;
        cmd[1]= 0x03; // Codice funzione modbus. 3=read 16bbit register
        cmd[2]= 0x01; // address MSB
        cmd[3]= 0x39; // address LSB
        cmd[4]= 0x0;  // MSB qty to read
        cmd[5]= 0x2;  // LSB qty to read

        byte[] frame = _BuildFrame(cmd);
        
        byte[] answer = myDev.QueryDevice(frame); 
        
        value= (myDev.BytesToUint32(answer,2))/1000; // va usata BytesToUInt16 
        return value;

      }
    



    }//class
}//namespace

      