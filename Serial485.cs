using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

//add
using System.IO.Ports;




namespace RaspHelloWord
{

   
 public    class Serial485
    {
        private SerialPort  _rs485;
        private byte[] _readBuffer;
        public Serial485(string portName, int baudRate,  Parity parity, int dataBits, StopBits stopBits)
        {
            try
            {
                _readBuffer = new byte[1024];
                _rs485 = new SerialPort(portName, baudRate, parity, dataBits,  stopBits);
                _rs485.ReadTimeout=2000;
                _rs485.Handshake = Handshake.None;
 
                _rs485.Open();
                _setReceiveMode();

                //_rs485.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived); //con rs485 la lettura asicrona crea problemi. Meglio sincrono 
                //_rs485.ReceivedBytesThreshold=7;
            }
            catch (Exception ex)
            { }


        }

        private void _setReceiveMode()
        {
            _rs485.RtsEnable=true;
            _rs485.DtrEnable=true;

        }

        private void _setTrasmitMode()
        {
            _rs485.RtsEnable=false;
            _rs485.DtrEnable=false;
            Task.Delay(200);
            //var t = Task.Run(async delegate
            //  {
            //     await Task.Delay(50);
            //     return 42;
            //  });
//
            //t.Wait(); // silence. for modbusRTU Must be > 3.5 * caracter time

        }

        public string AuroraGetSerialNumber(byte Address)
        {
           byte[] AuroraQryMsg = new byte[8];  
           byte[] frameOut = new byte[AuroraQryMsg.Length+2]  ; 
           byte[] frameOut1 = new byte[AuroraQryMsg.Length+2]  ; 
           
           byte[] frameIn = new byte[24]  ; 
           
           byte[] frameIn1 = new byte[8]  ; 
           byte[] frameIn2 = new byte[8]  ; 
           byte[] frameIn3 = new byte[8]  ; 
           
           int dataReaded1 = 0;
           int dataReaded2 = 0;
           int dataReaded3 = 0;
                    
           byte[] _crc = new byte[2];
           

           
//
            //t.Wait(); // silence. for modbusRTU Must be > 3.5 * caracter time
           
           
           Buffer.SetByte(AuroraQryMsg,0,Address);
           //Buffer.SetByte(AuroraQryMsg,1,0x63);  // comando get serial number
           Buffer.SetByte(AuroraQryMsg,1,0x3b);  // comando copiato  da un dump del sw power meter
           Buffer.SetByte(AuroraQryMsg,2,0x16);  // comando copiato  da un dump del sw power meter
           
           //_crc = GetCrc16_X25(AuroraQryMsg);
           _crc = CCITT_CRC16(AuroraQryMsg);
           Buffer.BlockCopy(AuroraQryMsg, 0, frameOut1, 0, AuroraQryMsg.Length);
           Buffer.BlockCopy(_crc, 0, frameOut1, AuroraQryMsg.Length, 2);

           Buffer.BlockCopy(AuroraQryMsg, 0, frameOut, 0, AuroraQryMsg.Length);
           Buffer.SetByte(frameOut,AuroraQryMsg.Length,0x9e);  // CrC copiato dal dump    
           Buffer.SetByte(frameOut,AuroraQryMsg.Length+1,0x72);  // CrC copiato dal dump        
           

           // _setTrasmitMode();
           var t1 = Task.Run(async delegate
           {
              await Task.Delay(20); // 20ms -> ~17byte@9600bps or 34byte@19200bps
              return 42;
           });
           t1.Wait();
          
           //Task.Delay(200);   //questo non funziona
            _rs485.Write(frameOut, 0, frameOut.Length );
          
           // _setReceiveMode();
           try
           {
                var t2 = Task.Run(async delegate
                {
                   await Task.Delay(250); // attendo 250ms per dar tempo al dispositivo di rispondere
                   return 42;
                });
                t2.Wait();
          
                dataReaded1 = _rs485.Read(frameIn1 , 0, frameIn1.Length);
                //Task.Delay(500);
                //Console.WriteLine("Lettura Nr 2");
                //dataReaded2 = _rs485.Read(frameIn2 , 0, frameIn2.Length);
                //Task.Delay(500);
                //Console.WriteLine("Lettura Nr 3");
                //dataReaded3 = _rs485.Read(frameIn3 , 0, frameIn3.Length);
               
                //byteReaded1 = _rs485.Read(frame1 , 0, frame1.Length);
                //byteReaded2 = _rs485.Read(frame2 , 0, frame2.Length);
                //
                //int _i=0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Nessuna Risposta o Risposta fuori tempo massimo");
            }

            Buffer.BlockCopy(frameIn1,0,frameIn,0,dataReaded1);
            //Buffer.BlockCopy(frameIn2,0,frameIn,dataReaded1,dataReaded2);
            //Buffer.BlockCopy(frameIn3,0,frameIn,dataReaded1+dataReaded2,dataReaded3);
            
           
           Console.WriteLine("DBG::> Inviato su rs485-USB >>>" + BitConverter.ToString(frameOut));
           Console.WriteLine("DBG::> Ricevuto su rs485-USB >>>" + BitConverter.ToString(frameIn));
           return frameIn.ToString();
        }


        private byte[] SwitchMsbLsb(ushort Val16bit)
        {
            byte[] _tmp = new byte[2];
            byte[] _modbus = new byte[2];

            _tmp = BitConverter.GetBytes(Val16bit);
            _modbus[1] = _tmp[0];
            _modbus[0] = _tmp[1];

            return _modbus;
        }

        public int sendRequest(byte IdDevice, byte Command, ushort StartAddr, ushort Howmany)
        {
            byte[] send = new byte[8];
            
            Buffer.SetByte(send, 0, IdDevice);
            Buffer.SetByte(send, 1, Command);
            Buffer.BlockCopy(SwitchMsbLsb(StartAddr), 0, send, 2, 2);
            Buffer.BlockCopy(SwitchMsbLsb(Howmany), 0, send, 4, 2);

            byte[] _crc = new byte[2];
            _crc = GetModbusCrc16(send);
            Buffer.BlockCopy(_crc, 0, send, 6, 2);
            //_ns.Write(send, 0, send.Length);
           
            _setTrasmitMode();
            _rs485.Write(send, 0, send.Length );
            _setReceiveMode();
             Console.WriteLine("485-USB Inviato:" + BitConverter.ToString(send));
           
            return send.Length;
        }

        public void SendModbusMsg(byte Addr, byte[] msg)
        {
            byte[] frame = new byte[msg.Length + 3];
            frame[0] = Addr;
            Buffer.BlockCopy(msg, 0, frame, 1, msg.Length);
            byte[] _crc = new byte[2];
            _crc = GetModbusCrc16(frame);
            Buffer.BlockCopy(_crc, 0, frame, msg.Length + 1, 2);
            //_ns.Write(frame, 0, frame.Length);

            _setTrasmitMode();
            _rs485.Write(frame, 0, frame.Length );
             _setReceiveMode();
             Console.WriteLine("485-USB Inviato:" + BitConverter.ToString(frame));

        }

public void sendTestAurora(byte IdDevice, byte Command)
        {
            byte[] send = new byte[10];
            //ushort _strAddr = 0x3001;
            //ushort _nr = 0x2;
            //byte idDevice = 0x02;
            Buffer.SetByte(send, 0, IdDevice);
            Buffer.SetByte(send, 1, Command);
           
            byte[] _crc = new byte[2];
            _crc = GetModbusCrc16(send);
            Buffer.BlockCopy(_crc, 0, send, 8, 2);
            _setTrasmitMode();
             _rs485.Write(send, 0, send.Length );
            //_setReceiveMode();
            
            Console.WriteLine("485-USB Inviato Aurora Request:" + BitConverter.ToString(send));

        }

 private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
 {
   int byteReaded;
   byte[] buff = new byte[_rs485.BytesToRead];
   byteReaded=_rs485.Read(buff, 0, _rs485.BytesToRead);
   var t = Task.Run(async delegate
              {
                 await Task.Delay(200);
                 return 42;
              });
              t.Wait();

   if (_rs485.BytesToRead>0) byteReaded=_rs485.Read(buff, 0, _rs485.BytesToRead); // read again
   Buffer.BlockCopy( buff,0,this._readBuffer,0,buff.Length);  

  Console.WriteLine("callback::485-USB Readed: " +BitConverter.ToString(buff,0));
   
}

        private byte[] _readModbusMsg(int pBufferLength=1024)
        {
             int byteReaded = 0;
             int byteReaded1 =0;
             int byteReaded2 =0;
            byte[] frame = new byte[pBufferLength];
            byte[] frame1 = new byte[pBufferLength];
            byte[] frame2 = new byte[pBufferLength];
            //_ns.ReadTimeout = (10000);
            
           _setReceiveMode();
            
            try
            {
                // inserisco un ritardo per dare tempo di ricevere tutta la risposta
                //Task.Delay(200);
                byteReaded = _rs485.Read(frame , 0, frame.Length);
                byteReaded1 = _rs485.Read(frame1 , 0, frame1.Length);
                byteReaded2 = _rs485.Read(frame2 , 0, frame2.Length);
                
                int _i=0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Nessuna Risposta o Risposta fuori tempo massimo");
            }

            byte[] fromdevice = new byte[byteReaded+byteReaded1+byteReaded2];
            Buffer.BlockCopy(frame, 0, fromdevice, 0, byteReaded);
            Buffer.BlockCopy(frame1, byteReaded , fromdevice, 0, byteReaded1);
            Buffer.BlockCopy(frame2, byteReaded+byteReaded1, fromdevice, 0, byteReaded2);
            

            //
            Console.WriteLine("485-USB Ricevuto:" + BitConverter.ToString(fromdevice));
            return fromdevice;
        }

  public Int16 ReadInt(byte[] data)
        {
            if (data.Length != 2) return -1;
            byte[] _tmp = new byte[2];
            _tmp[0] = data[1];
            _tmp[1] = data[0];

            return BitConverter.ToInt16(_tmp);
        }
        public void ReadModbusMsg(out byte[] readed)
        {
            readed = _readModbusMsg();
        }

        public void ReadModbusMsg()
        {
            _readModbusMsg();

        }




        

        /// <summary>
        /// Calcola il CRC di un frame MODBUS: addr+msg+CRC. Del buffer non considera gli ultimi due byte perchè destinati a contenere il CRC
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private byte[] GetModbusCrc16(byte[] bytes)
        {
            byte crcRegister_H = 0xFF, crcRegister_L = 0xFF; // presets a 16-bit register value 0xFFFF

            byte polynomialCode_H = 0xA0, polynomialCode_L = 0x01; // polynomial code 0xA001

            for (int i = 0; i < bytes.Length - 2; i++)
            {
                crcRegister_L = (byte)(crcRegister_L ^ bytes[i]);

                for (int j = 0; j < 8; j++)
                {
                    byte tempCRC_H = crcRegister_H;
                    byte tempCRC_L = crcRegister_L;

                    crcRegister_H = (byte)(crcRegister_H >> 1);
                    crcRegister_L = (byte)(crcRegister_L >> 1);
                    // Finally, a bit after the first should be the lower right front upper right: If the last digit is a high-low 1 right up front
                    if ((tempCRC_H & 0x01) == 0x01)
                    {
                        crcRegister_L = (byte)(crcRegister_L | 0x80);
                    }

                    if ((tempCRC_L & 0x01) == 0x01)
                    {
                        crcRegister_H = (byte)(crcRegister_H ^ polynomialCode_H);
                        crcRegister_L = (byte)(crcRegister_L ^ polynomialCode_L);
                    }
                }
            }

            return new byte[] { crcRegister_L, crcRegister_H };
        }


        // CRC16-X25 utilizzato dall'inverter Autora PowerOne
        // Il metodo restituisce il CRC già con i MSB e LSB invertiti in modo da essere 
        // pronto ad essere copiato nel frame da trasmettere
        private byte[] CCITT_CRC16(byte[] dataMsg)
        {
                ushort data;
                ushort crc = 0xFFFF;
                

                for (int j = 0; j < dataMsg.Length; j++)
                {
                    crc = (ushort)(crc ^ dataMsg[j]);
                    for (int i = 0; i < 8; i++)
                    {
                        if ((crc & 0x0001) == 1)
                            crc = (ushort)((crc >> 1) ^ 0x8408);
                        else
                            crc >>= 1;
                    }
                }
                crc = (ushort)~crc;
                data = crc;
                crc = (ushort)((crc << 8) ^ (data >> 8 & 0xFF));
                byte[] crcX25 = BitConverter.GetBytes(crc);
                //inverto MSB e LSB
                byte tmp = crcX25[0];
                crcX25[0]=crcX25[1];
                crcX25[1]= tmp;
                return crcX25;
                
        }

    }
}
