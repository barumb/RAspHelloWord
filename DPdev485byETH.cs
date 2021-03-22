using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;
using RaspHelloWord;
//add
using System.IO.Ports;
using System.Net.Sockets;

namespace RaspHelloWord
{
  public  class DPdev485byETH:DPdev485
   {
      
       private TcpClient _rs485;
      
       private NetworkStream _ns;
        private byte[] _rcvFrame;
       public DPdev485byETH(int baudRate,  Parity parity, int dataBits, StopBits stopBits, string COMorIP="", int port=0, int lenSndBuffer=32,int lenRecBuffer=32)
       {
         
        try
        {
           _rcvFrame = new byte[lenRecBuffer];
          _rs485 = new TcpClient(COMorIP, port);
          _ns = _rs485.GetStream();
          
          // qui andrebbe aggiunto il codice di impostazione seriale del convertiture RS485-ETH
          // per il momento ipotizziamo che sia già opportunamente configurato

                
        }
        catch (SocketException se)
        { 
          throw se;
        }
       }

       public override byte[] QueryDevice (byte[] pSendFrame)
       {
         byte[] _rcvMsg = new byte[1]{0x0}; // buffer contenente la risposta del device estratta dal buffer di ricezione
         int _nrByteReaded=0;
         
         
         var t1 = Task.Run(async delegate
           {
              await Task.Delay(20); // 20ms -> ~17byte@9600bps or 34byte@19200bps
              return 42;
           });
         t1.Wait();
         _ns.Write(pSendFrame, 0, pSendFrame.Length );
         try
         {
          var t2 = Task.Run(async delegate
          {
             await Task.Delay(250); // attendo 250ms per dar tempo al dispositivo di rispondere
             return 42;
          });
          t2.Wait();  // diamo tempo al device di rispondere. Forse andrebbe parametrizzato
          
          //_nrByteReaded= _rs485.Read(_rcvFrame , 0, _rcvFrame.Length);
          _nrByteReaded= _ns.Read(_rcvFrame , 0, _rcvFrame.Length);
          _rcvMsg = new byte[_nrByteReaded];
          Buffer.BlockCopy(_rcvFrame,0,_rcvMsg,0,_nrByteReaded);
           // Console.WriteLine("Ricevuto " + _nrByteReaded.ToString() + " bytes: " + BitConverter.ToString(_rcvMsg));
         }
         catch (Exception ex)
         {
             Console.WriteLine("Nessuna Risposta o Risposta fuori tempo massimo");
         }

         return _rcvMsg;

       }
       
   }//class

    
}//namespace