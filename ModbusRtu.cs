using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//add
using System.Net.Sockets;


namespace RaspHelloWord
{
    class ModbusRtu
    {
        private TcpClient _rs485;
        private NetworkStream _ns;


        public ModbusRtu(string System, int port)
        {
            try
            {
                _rs485 = new TcpClient(System, port);
                
                _ns = _rs485.GetStream();
                
            }
            catch (SocketException se)
            { }


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

        public void sendRequest(byte IdDevice, byte Command, ushort StartAddr, ushort Howmany)
        {
            byte[] send = new byte[8];
            //ushort _strAddr = 0x3001;
            //ushort _nr = 0x2;
            //byte idDevice = 0x02;
            Buffer.SetByte(send, 0, IdDevice);
            Buffer.SetByte(send, 1, Command);
            Buffer.BlockCopy(SwitchMsbLsb(StartAddr), 0, send, 2, 2);
            Buffer.BlockCopy(SwitchMsbLsb(Howmany), 0, send, 4, 2);

            byte[] _crc = new byte[2];
            _crc = GetModbusCrc16(send);
            Buffer.BlockCopy(_crc, 0, send, 6, 2);
            _ns.Write(send, 0, send.Length);
            
            Console.WriteLine("485-ETH Inviato:" + BitConverter.ToString(send));

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
            _ns.Write(send, 0, send.Length);
            
            Console.WriteLine("485-ETH Inviato Aurora Request:" + BitConverter.ToString(send));

        }


        public void SendModbusMsg(byte Addr, byte[] msg)
        {
            byte[] frame = new byte[msg.Length + 3];
            frame[0] = Addr;
            Buffer.BlockCopy(msg, 0, frame, 1, msg.Length);
            byte[] _crc = new byte[2];
            _crc = GetModbusCrc16(frame);
            Buffer.BlockCopy(_crc, 0, frame, msg.Length + 1, 2);
            _ns.Write(frame, 0, frame.Length);



        }

        private byte[]  _readModbusMsg()
        {
            int byteReaded = 0;
            byte[] frame = new byte[1024];
            _ns.ReadTimeout = (10000);
            try
            {
                byteReaded = _ns.Read(frame, 0, frame.Length);
            }
            catch (Exception t)
            {
                Console.WriteLine("Nessuna Risposta o Risposta fuori tempo massimo");
            }

            byte[] fromdevice = new byte[byteReaded];
            Buffer.BlockCopy(frame, 0, fromdevice, 0, byteReaded);

            //
            Console.WriteLine("485-ETH Ricevuto:" + BitConverter.ToString(fromdevice));
            return fromdevice;
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

            for (int i = 0; i < bytes.Length-2; i++)
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
    }
}
