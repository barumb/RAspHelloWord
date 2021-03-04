using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//aDD
using System.Net.Sockets;

namespace RaspHelloWord
{
    class AuroraPwrOneInverter
    {
        private TcpClient _rs485;
        private NetworkStream _ns;


        public AuroraPwrOneInverter(string System, int port)
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
    

    private void sendCMD(byte IdDevice, byte[] Command)
    {
        byte[] send = new byte[10];
        Buffer.SetByte(send, 0, IdDevice);
        Buffer.BlockCopy(Command, 0, send, 1, 7);
       
        byte[] _crc = new byte[2];
        _crc = GetModbusCrc16(send);
        Buffer.BlockCopy(_crc, 0, send, 8, 2);
        _ns.Write(send, 0, send.Length);
        Console.WriteLine("Inviato:" + BitConverter.ToString(send));

    }

        public void CmdDSPGridPower(byte IdAddress)
        {
            //read DSP, Grid Power, Globla
            byte[] Cmd = new byte[7] { 59, 3, 1, 0, 0, 0, 0 };
            sendCMD(IdAddress, Cmd);
        }

        public void CmdReadSN(byte IdAddress)
        {
            //read DSP, Grid Power, Globla
            byte[] Cmd = new byte[7] { 63, 0, 0, 0, 0, 0, 0 };
            sendCMD(IdAddress, Cmd);
        }

        public void ReadSn()
        {
            int byteReaded = 0;
            byte[] frame = new byte[8];
            _ns.ReadTimeout = (5000);
            try
            {
                byteReaded = _ns.Read(frame, 0, frame.Length);
                Console.WriteLine("Ricevuto:" + BitConverter.ToString(frame));
            }
            catch (Exception t)
            {
                Console.WriteLine("Nessuna Risposta o Risposta fuori tempo massimo");
            }


        }

        public void CmdDbg(byte IdAddress)
        {
            //read DSP, Grid Power, Globla
            byte[] Cmd = new byte[7] { 0x3f, 0, 0, 0, 0, 0, 0 };
            sendCMD(IdAddress, Cmd);
        }

        public void ReadDbg()
        {
            int byteReaded = 0;
            byte[] frame = new byte[8];
            _ns.ReadTimeout = (5000);
            try
            {
                byteReaded = _ns.Read(frame, 0, frame.Length);

                Console.WriteLine("Ricevuto:" + BitConverter.ToString(frame));
                
            }
            catch (Exception t)
            {
                Console.WriteLine("Nessuna Risposta o Risposta fuori tempo massimo");
            }


        }


    public void ReadDSPValue()
    {
        int byteReaded = 0;
        byte[] frame = new byte[8];
        _ns.ReadTimeout = (5000);
        try
        {
            byteReaded = _ns.Read(frame, 0, frame.Length);

            float _val;
            byte[] _fload = new byte[4];
            Buffer.BlockCopy(frame, 2, _fload, 0, 4);

            _val = BitConverter.ToSingle(_fload, 0);
            Console.WriteLine("Ricevuto:" + BitConverter.ToString(frame));
            Console.WriteLine("Valore:" + _val.ToString());

            }
            catch (Exception t)
        {
            Console.WriteLine("Nessuna Risposta o Risposta fuori tempo massimo");
        }

        
        }

    }

}

