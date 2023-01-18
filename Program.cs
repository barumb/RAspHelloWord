using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;
using RaspHelloWord;
//add
using System.IO.Ports;

namespace console1
{
    class Program
    {
        static GpioController controller;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello Raspberry! press Enter to start");
            //Console.ReadLine();

            //Test01();  // led 
            //Test02();  // temperatura
            //Test03();    // relè
            //Test04();    //temperatura + relè
            //Test05();   // Test lettura pulsante asincrona
            //Test06();   //lettura sonde + comando asincrono rele
            //Test07();     // RS485 by Ethernet (modbus)
            //Test08();      // RS485 by ethernet inverter Aurora
            //Test09();       // RS485 Power meter OR-WE-515
            //Test10();      // Gruppo elettrogeno Sauro tramite Moxxa5604 
            //Test11();       // Usb->485 Power Meter OR-WE-515
            //Test12();       // Usb ->485 to 485/232-eth 
            //Lab13(); // send by rs485 an receive on rs485-rth2
            //Lab14();   // Abstact Facroty for device
            Lab15();   // Abstact Factory for GE by serial
        }



        static PinChangeEventHandler onPushButton = (object sender, PinValueChangedEventArgs arg) =>
       {
           Console.WriteLine("call back ###> Pin "+ arg.PinNumber.ToString()+" changed to "+arg.ChangeType.ToString());
           //((GpioController)sender)
          
       };


        static PinChangeEventHandler onPushButton_switchrele = (object sender, PinValueChangedEventArgs arg) =>
        {
            var pinch1 = 15;
            var pinch2 = 13;
            var pinch3 = 11;
            var pinch4 = 16;
            PinValue PinvalueSwitch;

            //Console.WriteLine("call back ###> Pin " + arg.PinNumber.ToString() + " changed to " + arg.ChangeType.ToString() +"Type of sender: " + sender.GetType().ToString());


            
            PinvalueSwitch = SwitchedPinValue(controller, pinch1);
            //Console.WriteLine("dbg::> Try tu switch to " + PinvalueSwitch.ToString() );
            controller.Write(pinch1, PinvalueSwitch);

            PinvalueSwitch = SwitchedPinValue(controller, pinch2);
            //Console.WriteLine("dbg::> Try tu switch to " + PinvalueSwitch.ToString());
            controller.Write(pinch2, PinvalueSwitch);

            PinvalueSwitch = SwitchedPinValue(controller, pinch3);
            //Console.WriteLine("dbg::> Try tu switch to " + PinvalueSwitch.ToString());
            controller.Write(pinch3, PinvalueSwitch);

            PinvalueSwitch = SwitchedPinValue(controller, pinch4);
            //Console.WriteLine("dbg::> Try tu switch to " + PinvalueSwitch.ToString());
            controller.Write(pinch4, PinvalueSwitch);

        };

        static private PinValue SwitchedPinValue(GpioController device, int PinNumber)
        {
            //Console.WriteLine("dbg::> SwitchedPinValue(....)  pin " + PinNumber.ToString() + "status " + device.Read(PinNumber).ToString());
            PinValue PinvalueSwitch = (device.Read(PinNumber) == PinValue.Low) ? PinValue.High : PinValue.Low;
           
            return PinvalueSwitch;

        }

public static void Lab15()
{
    DPbusFactory myBusUSB = new DPbusUSBFactory();
    //DPOrnoWE515 _powerMeterUSB = new DPOrnoWE515(myBusUSB, 1, 9600, Parity.None, 8, StopBits.One, "COM3");
    DPdev485byUSB _GE485 = new DPdev485byUSB(9600, Parity.None, 8, StopBits.One,"COM3");


     //_sauroGE.sendRaw(serialSTART);
     //_sauroGE.ReadModbusMsg();
     byte[] cmdMmodbusRTU = new byte[8] { 0x01, 0x03, 0x00, 0x42, 0x00, 0x10, 0xe4, 0x12 };
            byte[] CRC = _GE485.Modbus_Crc16(cmdMmodbusRTU);
     byte[] frame = new byte[10];

     Buffer.BlockCopy(CRC, 0, frame, cmdMmodbusRTU.Length + 1, 2);


     Console.WriteLine("TO URB:>>{0}", frame.ToString());
     byte[] _response = _GE485.QueryDevice(frame);
     Console.WriteLine("From URB:>>{0}",_response.ToString());
     
}

public static void Lab14()
{
    DPbusFactory myBusUSB = new DPbusUSBFactory();
    DPAuroraInverter _inverterUSB = new DPAuroraInverter(myBusUSB, 2,19200,Parity.Even,8,StopBits.One, "COM3");
    _inverterUSB.ReadFrequency();
    _inverterUSB.ReadGridPower();
    _inverterUSB.CumulateTotalEnergy();
    _inverterUSB.CumulateCurrentDayEnergy();
    _inverterUSB.ReadGridCurrent();
    _inverterUSB.ReadGridVoltage();
    
    DPOrnoWE515 _powerMeterUSB = new DPOrnoWE515(myBusUSB,1,9600,Parity.None,8,StopBits.One,"COM3");
    _powerMeterUSB.ReadActivePower();
    _powerMeterUSB.ReadVoltage();
    _powerMeterUSB.ReadCurrent();


    DPbusFactory myBusEth = new DPbusETHFactory();
    DPAuroraInverter _inverterEth = new DPAuroraInverter(myBusEth, 2,19200,Parity.Even,8,StopBits.One, "192.168.0.232",4001);
    _inverterEth.ReadFrequency();
    _inverterEth.ReadGridPower();
    _inverterEth.CumulateTotalEnergy();
    _inverterEth.CumulateCurrentDayEnergy();
    _inverterEth.ReadGridCurrent();
    _inverterEth.ReadGridVoltage();

    DPOrnoWE515 _powerMeterEth = new DPOrnoWE515(myBusEth,1,9600,Parity.None,8,StopBits.One,"192.168.0.232",4001);
    _powerMeterEth.ReadActivePower();
    _powerMeterEth.ReadVoltage();
    _powerMeterEth.ReadCurrent();


}



public static void Lab13()
{
    bool _loop=true;
    string _r;

    Serial485 com = new Serial485("COM3",9600,System.IO.Ports.Parity.None, 8,System.IO.Ports.StopBits.One);
    ModbusRtu net = new ModbusRtu("192.168.0.237", 4001);
    
    com.sendRequest(0x01,0x03,0x131,0x1);
    net.ReadModbusMsg();
//
    net.sendRequest(0x01,0x03,0x131,0x1);
    com.ReadModbusMsg(); // OCCHIO!!! sembra che al messaggio ricevudo venda messo in testa un byte di indirizzo e in coda ricalcolato il CRC!!!
    
    


   
//   Console.WriteLine("Press 1 key to send Request usb->net \n Press 2 to send Request net->usb. \nPress X to exit");
//   while( _loop==true)
//   {
//       _r=Console.ReadLine();
//       switch (_r.ToUpper())
//       {
//           case "X": 
//             _loop=false;    
//             break;
//           case "1": _loop=false;
//                 com.sendRequest(0x01,0x03,0x131,0x1);
//                break;
//           case "2": _loop=false;
//                 net.sendRequest(0x01,0x03,0x131,0x1);
//                break;
//           default:
//                 Console.WriteLine(_r + "Comando non riconosciuto");
//                 break;
//       }
//    }
  Console.WriteLine("exiting....");
}

public static void Test12()
{
/*
01:22:47:240  Write: 01 03 01 31 00 01 D4 39 
01:22:47:562  Read: 01 03 02 57 12 06 79 
01:22:47:894  Over
*/
Serial485 _ornoWE515 = new Serial485("COM3", 9600, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One  );
            byte[] send = new byte[6];
            byte[] sent;
            byte[] readed;
            byte _cmd;
            ushort _strAddr;
            ushort _nr;
            byte idDevice;

            byte[] _intValue = new byte[2];


            idDevice = 0x1;  //default address
            _cmd = 0x03;   //read
             _nr = 0x01;   // nr registri da leggere
            
            
            _strAddr = 0x131;  //tensione
            _nr = 0x01;
            _ornoWE515.sendRequest(idDevice, _cmd, _strAddr, _nr);
            
            _ornoWE515.ReadModbusMsg();

            Console.WriteLine("In attesa dei dati......Premi invio per terminare");
            Console.ReadLine();
           

}


        /*
         * 
         Modbus Coils	    Bits,binary values,flags    00001
         Digital Inputs	    Binary inputs	            10001
         Analog Inputs	    Binary inputs	            30001
         Modbus Registers	Analog values, variables	40001
         * 
         * 
         */

 public static void Test11()
        {
            Serial485 _ornoWE515 = new Serial485("COM3", 9600, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One  );
            byte[] send = new byte[6];
            byte[] sent;
            byte[] readed;
            byte _cmd;
            ushort _strAddr;
            ushort _nr;
            byte idDevice;

            byte[] _intValue = new byte[2];


            // Test comandi Conve ETH-RS485 - Nessuna risposta
            //byte[] mybuff = new byte[3+2];
            //mybuff[0] = 0x2b; //'+';
            //mybuff[1] = 0x2b;// '+';
            //mybuff[2] = 0x2b;// '+';
            //mybuff[3] = 0x0D;// CR;
            //mybuff[4] = 0x0A;// LF;
            //_ornoWE515.sendBadRequest(mybuff);
            //_ornoWE515.ReadModbusMsg(out readed);
            
            //byte[] mycmd = new byte[1+2];
            //mycmd[0] = 0x61; // 'a';
            //mycmd[0] = 0x0D; // 'CR';
            //mycmd[0] = 0x0A; // 'LF';
            //_ornoWE515.sendBadRequest(mycmd);
            //_ornoWE515.ReadModbusMsg(out readed);



            idDevice = 0x1;  //default address
            _cmd = 0x03;   //read
             _nr = 0x01;   // nr registri da leggere
            
            
            _strAddr = 0x131;  //tensione
            _nr = 0x01;
            _ornoWE515.sendRequest(idDevice, _cmd, _strAddr, _nr);
            
            _ornoWE515.ReadModbusMsg(out readed);
            Console.WriteLine("Buffer Addr 0x131> len="+ readed.Length.ToString() + " data=" + BitConverter.ToString(readed, 0).ToString());

            if (readed.Length > 0)
            {
                Buffer.BlockCopy(readed, 0, _intValue, 0, 2);
                Console.WriteLine("Tensione(UINT 16bit start index 0)=" + _ornoWE515.ReadInt(_intValue).ToString());

                Buffer.BlockCopy(readed, 1, _intValue, 0, 2);
                Console.WriteLine("Tensione(UINT 16bit start index 1)=" + _ornoWE515.ReadInt(_intValue).ToString());

                Buffer.BlockCopy(readed, 2, _intValue, 0, 2);
                Console.WriteLine("Tensione(UINT 16bit start index 2)=" + _ornoWE515.ReadInt(_intValue).ToString());

                Buffer.BlockCopy(readed, 3, _intValue, 0, 2);
                Console.WriteLine("Tensione(UINT 16bit start index 3)=" + _ornoWE515.ReadInt(_intValue).ToString());

                Buffer.BlockCopy(readed, 4, _intValue, 0, 2);
                Console.WriteLine("Tensione(UINT 16bit start index 4)=" + _ornoWE515.ReadInt(_intValue).ToString());
            }


            //_strAddr = 0x131;  //tensione
            //_nr = 0x02;  // nr registri da leggere
            //_ornoWE515.sendRequest(idDevice, _cmd, _strAddr, _nr);
            //_ornoWE515.ReadModbusMsg(out readed);
            //Console.WriteLine("Buffer Addr 0x131=" + BitConverter.ToString(readed, 0).ToString());
            //
            //Buffer.BlockCopy(readed, 0, _intValue, 0, 2);
            //Console.WriteLine("Tensione(UINT 16bit start index 0)=" + _ornoWE515.ReadInt(_intValue).ToString());
            //
            //Buffer.BlockCopy(readed, 1, _intValue, 0, 2);
            //Console.WriteLine("Tensione(UINT 16bit start index 1)=" + _ornoWE515.ReadInt(_intValue).ToString());



            /*
            _strAddr = 0x139;
            _nr = 0x02;
            _ornoWE515.sendRequest(idDevice, _cmd, _strAddr, _nr);
            _ornoWE515.ReadModbusMsg(out readed);
            Console.WriteLine("Buffer Addr 0x139=" + BitConverter.ToString(readed, 0).ToString());
            Console.WriteLine("Corrente(UINT 16bit)=" + BitConverter.ToUInt16(readed, 1).ToString());
            Console.WriteLine("Corrente(UINT 16bit)=" + BitConverter.ToUInt16(readed, 2).ToString());

            _strAddr = 0x3001;
            _nr = 0xFF;
            _ornoWE515.sendRequest(idDevice, _cmd, _strAddr, _nr);
            _ornoWE515.ReadModbusMsg();
            Console.WriteLine("Buffer=" + BitConverter.ToString(readed, 0).ToString());

            idDevice = 0x2;  //default address
            _strAddr = 0x3001;
            _nr = 0xFF;
            _ornoWE515.sendRequest(idDevice, _cmd, _strAddr, _nr);
            _ornoWE515.ReadModbusMsg();
            Console.WriteLine("Buffer=" + BitConverter.ToString(readed, 0).ToString());
            */

        }


        public static void Test10()
        {
            PragmaGE _sauroGE = new PragmaGE("172.20.48.40", 4001);
            //PragmaGE _sauroGE = new PragmaGE("192.168.0.236", 4001);
            byte[] cmdMmodbusRTU = new byte[6];
            byte[] serialSTART = new byte[4];
            byte[] serialEND = new byte[3];

            serialSTART = new byte[4] { 0x12, 0x02, 0x01, 0x01 };
            serialEND = new byte[4] { 0x12, 0x02, 0x01, 0x00 };


            //_sauroGE.sendRaw(serialSTART);
            //_sauroGE.ReadModbusMsg();
            cmdMmodbusRTU = new byte[8] { 0x01, 0x03, 0x00, 0x42, 0x00, 0x10, 0xe4, 0x12 };
            _sauroGE.sendRaw(cmdMmodbusRTU);
            _sauroGE.ReadModbusMsg();
            //_sauroGE.sendRaw(serialEND);
            //_sauroGE.ReadModbusMsg();

            cmdMmodbusRTU = new byte[8] { 0x01, 0x04, 0x00, 0x00, 0x00, 0x28, 0xf0, 0x14 };
            _sauroGE.sendRaw(cmdMmodbusRTU);
            _sauroGE.ReadModbusMsg();



            byte[] sent;
            byte[] readed;
            byte _cmd;
            ushort _strAddr;
            ushort _nr;
            byte idDevice;

            byte[] _intValue = new byte[2];


        }

       

        //Power meteer ORNO-WE-515
#region  RegisterORNO-WE-515
        /*
        Frequency:       0x130, // 16 bit, 0.01Hz

		VoltageL1:       0x131, // 16 bit, 0.01V
		CurrentL1:       0x139, // 16 bit, 0.001A
		PowerL1:         0x140, // 32 bit, 0.001kW
		ReactivePowerL1: 0x148, // 32 bit, 0.001kvar
		ApparentPowerL1: 0x150, // 32 bit, 0.001kva
		CosphiL1:        0x158, // 16 bit, 0,001

		VoltageL2:       0x132, // 16 bit, 0.01V
		CurrentL2:       0x13B, // 32 bit, 0.001A
		PowerL2:         0x142, // 32 bit, 0.001kW
		ReactivePowerL2: 0x14A, // 32 bit, 0.001kvar
		ApparentPowerL2: 0x152, // 32 bit, 0.001kva
		CosphiL2:        0x159, // 16 bit, 0,001

		VoltageL3:       0x133, // 16 bit, 0.01V
		CurrentL3:       0x13D, // 32 bit, 0.001A
		PowerL3:         0x144, // 32 bit, 0.001kW
		ReactivePowerL3: 0x14C, // 32 bit, 0.001kvar
		ApparentPowerL3: 0x154, // 32 bit, 0.001kva
		CosphiL3:        0x15A, // 16 bit, 0,001

		Power:         0x146, // 32 bit, 0.001kW
		ReactivePower: 0x14E, // 32 bit, 0.001kvar
		ApparentPower: 0x156, // 32 bit, 0.001kva
		Cosphi:        0x15B, // 16 bit, 0.001

		Sum:   0xA000, //32 Bit, 0.01kwh
		SumT1: 0xA002, //32 Bit, 0.01kwh
		SumT2: 0xA004, //32 Bit, 0.01kwh
		//		SumT3:           0xA006, //32 Bit, 0.01kwh // currently not supported
		//		SumT4:           0xA008, //32 Bit, 0.01kwh // currently not supported
		ReactiveSum:   0xA01E, //32 Bit, 0.01kvarh
		ReactiveSumT1: 0xA020, //32 Bit, 0.01kvarh
		ReactiveSumT2: 0xA022, //32 Bit, 0.01kvarh
		//		ReactiveSumT3:   0xA024, //32 Bit, 0.01kvarh // currently not supported
		//		ReactiveSumT4:   0xA026, //32 Bit, 0.01kvarh // currently not supported
      */

        #endregion 


        public static void Test09()
        {
            //PowerMeter _ornoWE515 = new PowerMeter("192.168.0.237", 4001);
           
            //aurora
            Serial485 com = new Serial485("COM3",19200,System.IO.Ports.Parity.Even, 8,System.IO.Ports.StopBits.One);
            //Serial485 com = new Serial485("COM3",9600,System.IO.Ports.Parity.Even, 8,System.IO.Ports.StopBits.One);
            //ModbusRtu net = new ModbusRtu("192.168.0.237", 4001);



            // read voltage
            //byte[] Cmd = new byte[7] { 63, 0, 0, 0, 0, 0, 0 };
            Task.Delay(200);
            com.sendTestAurora(0x02,0x63);
            //Task.Delay(200);
            com.ReadModbusMsg();
            return;

            Task.Delay(200);    
            // read voltage
            com.sendRequest(0x01,0x03,0x131,0x1);
            Task.Delay(200);
            com.ReadModbusMsg();

           
            /*
            23:39:47:875  Write: 01 03 01 40 00 02 C4 23 
            23:39:48:297  Read: 01 03 04 00 00 00 00 FA 33 
            23:39:48:642  Over
            */
            //read active power
            Task.Delay(150);
            com.sendRequest(0x01,0x03,0x140,0x2);
            Task.Delay(200);
            com.ReadModbusMsg();
           
                   
           //read active power
            Task.Delay(200);
            com.sendRequest(0x01,0x03,0x146,0x2);
            Task.Delay(200);
            com.ReadModbusMsg();
           




           // net.sendRequest(0x01,0x03,0x131,0x1);
           // net.ReadModbusMsg(); // OCCHIO!!! sembra che al messaggio ricevudo venda messo in testa un byte di indirizzo e in coda ricalcolato il CRC!!!
    
           



          //  idDevice = 0x01;  //default address
          //  _cmd = 0x03;   //read
          //   _nr = 0x01;   // nr registri da leggere
          //  
          //  
          //  _strAddr = 0x131;  //tensione
          //  _nr = 0x01;
          //  sent=_ornoWE515.sendRequest(idDevice, _cmd, _strAddr, _nr);
          //  Console.WriteLine("Inviato>" + BitConverter.ToString(sent));
          //  _ornoWE515.ReadModbusMsg(out readed);
          //  Console.WriteLine("Buffer Addr 0x131> len="+ readed.Length.ToString() + " data=" + BitConverter.ToString(readed, 0).ToString());
//
          //  if (readed.Length > 0)
          //  {
          //      Buffer.BlockCopy(readed, 0, _intValue, 0, 2);
          //      Console.WriteLine("Tensione(UINT 16bit start index 0)=" + _ornoWE515.ReadInt(_intValue).ToString());
//
          //      Buffer.BlockCopy(readed, 1, _intValue, 0, 2);
          //      Console.WriteLine("Tensione(UINT 16bit start index 1)=" + _ornoWE515.ReadInt(_intValue).ToString());
//
          //      Buffer.BlockCopy(readed, 2, _intValue, 0, 2);
          //      Console.WriteLine("Tensione(UINT 16bit start index 2)=" + _ornoWE515.ReadInt(_intValue).ToString());
//
          //      Buffer.BlockCopy(readed, 3, _intValue, 0, 2);
          //      Console.WriteLine("Tensione(UINT 16bit start index 3)=" + _ornoWE515.ReadInt(_intValue).ToString());
//
          //      Buffer.BlockCopy(readed, 4, _intValue, 0, 2);
          //      Console.WriteLine("Tensione(UINT 16bit start index 4)=" + _ornoWE515.ReadInt(_intValue).ToString());
          //  }
//
//
          //  //_strAddr = 0x131;  //tensione
          //  //_nr = 0x02;  // nr registri da leggere
          //  //_ornoWE515.sendRequest(idDevice, _cmd, _strAddr, _nr);
          //  //_ornoWE515.ReadModbusMsg(out readed);
          //  //Console.WriteLine("Buffer Addr 0x131=" + BitConverter.ToString(readed, 0).ToString());
          //  //
          //  //Buffer.BlockCopy(readed, 0, _intValue, 0, 2);
          //  //Console.WriteLine("Tensione(UINT 16bit start index 0)=" + _ornoWE515.ReadInt(_intValue).ToString());
          //  //
          //  //Buffer.BlockCopy(readed, 1, _intValue, 0, 2);
          //  //Console.WriteLine("Tensione(UINT 16bit start index 1)=" + _ornoWE515.ReadInt(_intValue).ToString());
//
//
//
          //  /*
          //  _strAddr = 0x139;
          //  _nr = 0x02;
          //  _ornoWE515.sendRequest(idDevice, _cmd, _strAddr, _nr);
          //  _ornoWE515.ReadModbusMsg(out readed);
          //  Console.WriteLine("Buffer Addr 0x139=" + BitConverter.ToString(readed, 0).ToString());
          //  Console.WriteLine("Corrente(UINT 16bit)=" + BitConverter.ToUInt16(readed, 1).ToString());
          //  Console.WriteLine("Corrente(UINT 16bit)=" + BitConverter.ToUInt16(readed, 2).ToString());
//
          //  _strAddr = 0x3001;
          //  _nr = 0xFF;
          //  _ornoWE515.sendRequest(idDevice, _cmd, _strAddr, _nr);
          //  _ornoWE515.ReadModbusMsg();
          //  Console.WriteLine("Buffer=" + BitConverter.ToString(readed, 0).ToString());
//
          //  idDevice = 0x2;  //default address
          //  _strAddr = 0x3001;
          //  _nr = 0xFF;
          //  _ornoWE515.sendRequest(idDevice, _cmd, _strAddr, _nr);
          //  _ornoWE515.ReadModbusMsg();
          //  Console.WriteLine("Buffer=" + BitConverter.ToString(readed, 0).ToString());
          //  */

        }

        public static void Test08()
        {
             //aurora
            Serial485 com = new Serial485("COM3",19200,System.IO.Ports.Parity.None, 8,System.IO.Ports.StopBits.One);
            //Serial485 com = new Serial485("COM3",9600,System.IO.Ports.Parity.Even, 8,System.IO.Ports.StopBits.One);
            //ModbusRtu net = new ModbusRtu("192.168.0.237", 4001);
            byte[] value;
           


            value=com.AuroraGetSerialNumber(0x02);

            for (int i=0; i<200 ; i++)
            {
            value=com.AuroraGetGridVoltage(0x2, true);
            Console.WriteLine("Tensione= " + BytesToSingle(value,2).ToString() + " V"); 
            value=com.AuroraGetGridCurrent(0x2, true);
            Console.WriteLine("Corrente= " + BytesToSingle(value,2).ToString() + " A"); 
            
            value=com.AuroraGetGridPower(0x2,true);
            Console.WriteLine("Potenza= " + BytesToSingle(value,2).ToString() + " W"); 
            }
            return;


            
        }

        //Lettura dati da inverter tramite RS485 utilizzando un convertirore RS485-Eth con rete di polarizzazione
        public static void Test07()
        {
            ModbusRtu _inverter = new ModbusRtu("172.20.48.40", 4001);
            byte[] send = new byte[6];
            byte    _cmd = 0x02;
            ushort  _strAddr = 0x3001;
            ushort  _nr = 0x80;
            byte    idDevice = 0x02;
            
            _inverter.sendRequest(idDevice, _cmd , _strAddr, _nr);
            _inverter.ReadModbusMsg();

            _cmd = 0x02;
            _strAddr = 0x4001;
            _nr = 0xF0;
            idDevice = 0x02;

            _inverter.sendRequest(idDevice, _cmd, _strAddr, _nr);
            _inverter.ReadModbusMsg();



        }

        //lettura sonde + attivazione 4canali relè da pulsante
        public static void Test06()
        {
            controller = new GpioController(PinNumberingScheme.Board);

            var lightTime = 500;

            var pinch1 = 15;
            var pinch2 = 13;
            var pinch3 = 11;
            var pinch4 = 16; //provo a usare il quarto canale sul pin 16 invece del 7 che è usato per il bus 1-wire

            var pinIN1 = 18; // PIn 18 input. Pulsante
            //var pinch4 = 7;   //questo serve per leggere il bus 1-wire

            Console.WriteLine("Test Lettura Temperatura + Relay 3 Channels");

            controller.OpenPin(pinch4, PinMode.Output);
            controller.SetPinMode(pinch4, PinMode.Output); //per impostare il pin deve essre prima "open"


            controller.OpenPin(pinch1, PinMode.Output);
            controller.SetPinMode(pinch1, PinMode.Output); //per impostare il pin deve essre prima "open"


            controller.OpenPin(pinch2, PinMode.Output);
            controller.SetPinMode(pinch2, PinMode.Output); //per impostare il pin deve essre prima "open"


            controller.OpenPin(pinch3, PinMode.Output);
            controller.SetPinMode(pinch3, PinMode.Output); //per impostare il pin deve essre prima "open"

            controller.OpenPin(pinIN1, PinMode.InputPullUp);
            controller.SetPinMode(pinIN1, PinMode.InputPullUp); //provo a impostare il pin prima di "aprirlo". Non funziona e genera runtime error

            //test gestione INPUT con chimate asincrone 
            controller.RegisterCallbackForPinValueChangedEvent(pinIN1, PinEventTypes.Rising, onPushButton_switchrele);


            string[] listaSonde = System.IO.Directory.GetDirectories("/sys/bus/w1/devices/", "28-*"); //   GetFiles("/sys/bus/w1/devices/","28-*");




            //System.IO.StreamReader[] sonda = new System.IO.StreamReader[listaSonde.GetLength(0)];
            int nrTempProdFound = listaSonde.GetLength(0);
            Console.WriteLine(nrTempProdFound.ToString() + " sonde trovate");
            List<TempProbe> ProbesTemp = new List<TempProbe>();
            for (int i = 0; i < nrTempProdFound; i++)
            {
                //sonda[i] = new System.IO.StreamReader(listaSonde[i]);
                Console.WriteLine("inizializzo " + listaSonde[i]);
                ProbesTemp.Add(new TempProbe(listaSonde[i], 0));

            }

            //TempProbe sonda1 = new TempProbe("28-012033bfd29c", 0);
            //TempProbe sonda2 = new TempProbe("28-012033aa416f", 0.5m);   // suffisso m per dichiarare 0.5 decimal al compilatore

            Console.WriteLine("Inizio Attività di acquisizione e controllo");
            string line;

            //var pin = 10;
            //controller.OpenPin(pin, PinMode.Output);

            try
            {
                while (true)
                {
                   
                    Console.WriteLine("\nInizio lettura sonde " + DateTime.Now.ToString());
                    line = "";
                    foreach (TempProbe probe in ProbesTemp)
                    {
                        line += "ID " + probe.Id + " rawtemp= " + probe.RawTemp().ToString() + "\t\t temp=" + probe.Temp().ToString() + "\n";

                    }
                    Console.WriteLine("Fine lettura sonde " + DateTime.Now.ToString());
                    Console.WriteLine(line);
                   
                    Thread.Sleep(lightTime * 10);
                   
                }
            }
            finally
            {
                controller.ClosePin(pinch1);
                controller.ClosePin(pinch2);
                controller.ClosePin(pinch3);
                controller.ClosePin(pinch4);
               // controller.ClosePin(pin);

            }



        }

        // test pressione pulsante 
        public static void Test05()
        {
            GpioController controller = new GpioController(PinNumberingScheme.Board);
            var pinIN1 = 18; // PIn 18 input. Pulsante
            var lightTime = 1000;
            Console.WriteLine("Test Lettura Pulsante");


            controller.OpenPin(pinIN1, PinMode.InputPullDown);
            controller.SetPinMode(pinIN1, PinMode.InputPullDown); //provo a impostare il pin prima di "aprirlo". Non funziona e genera runtime error

            controller.RegisterCallbackForPinValueChangedEvent(pinIN1, PinEventTypes.Rising, onPushButton);

            
            try
            {
                while (true)
                {
                    

                    Thread.Sleep(lightTime);


                    //Console.WriteLine("Attendo la pressione del pulsante " + DateTime.Now.ToString());
                    Console.WriteLine("Pin18 in stato " + controller.Read(pinIN1).ToString());

                                  }
            }
            finally
            {
             
                controller.ClosePin(pinIN1);

            }


        }


        //Inizializzazione di tutte le sonde che vengono trovati nel sistema
        public static void Test04()
        {
            GpioController controller = new GpioController(PinNumberingScheme.Board);

            var lightTime = 500;

            var pinch1 = 15;
            var pinch2 = 13;
            var pinch3 = 11;
            var pinch4 = 16; //provo a usare il quarto canale sul pin 16 invece del 7 che è usato per il bus 1-wire

            var pinIN1 = 18; // PIn 18 input. Pulsante
            //var pinch4 = 7;   //questo serve per leggere il bus 1-wire

            Console.WriteLine("Test Lettura Temperatura + Relay 3 Channels");


            controller.OpenPin(pinIN1, PinMode.InputPullUp);
            controller.SetPinMode(pinIN1, PinMode.InputPullUp); //provo a impostare il pin prima di "aprirlo". Non funziona e genera runtime error
            

            //test gestione INPUT con chimate asincrone 
            controller.RegisterCallbackForPinValueChangedEvent(pinIN1, PinEventTypes.Rising, onPushButton);

            controller.OpenPin(pinch4, PinMode.Output);
            controller.SetPinMode(pinch4, PinMode.Output); //per impostare il pin deve essre prima "open"


            controller.OpenPin(pinch1, PinMode.Output);
            controller.SetPinMode(pinch1, PinMode.Output); //per impostare il pin deve essre prima "open"


            controller.OpenPin(pinch2, PinMode.Output);
            controller.SetPinMode(pinch2, PinMode.Output); //per impostare il pin deve essre prima "open"


            controller.OpenPin(pinch3, PinMode.Output);
            controller.SetPinMode(pinch3, PinMode.Output); //per impostare il pin deve essre prima "open"

            string[] listaSonde = System.IO.Directory.GetDirectories("/sys/bus/w1/devices/", "28-*"); //   GetFiles("/sys/bus/w1/devices/","28-*");


            

            //System.IO.StreamReader[] sonda = new System.IO.StreamReader[listaSonde.GetLength(0)];
            int nrTempProdFound = listaSonde.GetLength(0);
            Console.WriteLine(nrTempProdFound.ToString() +  " sonde trovate");
            List<TempProbe> ProbesTemp= new List<TempProbe>();
            for (int i = 0; i < nrTempProdFound; i++)
            {
                //sonda[i] = new System.IO.StreamReader(listaSonde[i]);
                Console.WriteLine("inizializzo " + listaSonde[i] );
                ProbesTemp.Add(new TempProbe(listaSonde[i], 0));
              
            }

            //TempProbe sonda1 = new TempProbe("28-012033bfd29c", 0);
            //TempProbe sonda2 = new TempProbe("28-012033aa416f", 0.5m);   // suffisso m per dichiarare 0.5 decimal al compilatore

            Console.WriteLine("Inizio Attività di acquisizione e controllo");
            string line;

            var pin = 10;
            controller.OpenPin(pin, PinMode.Output);

            try
            {
                while (true)
                {
                    Console.WriteLine("ch1 ON");
                    controller.Write(pinch1, PinValue.High);
                    

                    Console.WriteLine("ch2 ON");
                    controller.Write(pinch2, PinValue.High);
                   

                    Console.WriteLine("ch3 ON");
                    controller.Write(pinch3, PinValue.High);


                    Console.WriteLine("ch4 ON");
                    controller.Write(pinch4, PinValue.High);


                    controller.Write(pin, PinValue.High); //led 10 on
                    Thread.Sleep(lightTime);

                   
                    Console.WriteLine("Inizio lettura sonde "+ DateTime.Now.ToString());
                    line = "";
                    foreach(TempProbe probe in ProbesTemp)
                    {
                        line += "ID "+probe.Id +" rawtemp= " + probe.RawTemp().ToString() + "\t\t temp=" + probe.Temp().ToString() + "\n";

                    }
                    Console.WriteLine("Fine lettura sonde " + DateTime.Now.ToString());
                    Console.WriteLine(line);
                    //line = "rawtemp= " + sonda1.RawTemp().ToString() + "\t\t temp=" + sonda1.Temp().ToString();
                    //Console.WriteLine("sonda1->" + line);
                    //
                    //line = "rawtemp= " + sonda2.RawTemp().ToString() + "\t\t temp=" + sonda2.Temp().ToString();
                    //Console.WriteLine("sonda2->" + line);

                    



                    controller.Write(pin, PinValue.Low);  // led 10 off

                    controller.Write(pinch1, PinValue.Low);
                    Console.WriteLine("ch1 OFF");
                    controller.Write(pinch2, PinValue.Low);
                    Console.WriteLine("ch2 OFF");
                    controller.Write(pinch3, PinValue.Low);
                    Console.WriteLine("ch3 OFF");
                    controller.Write(pinch4, PinValue.Low);
                    Console.WriteLine("ch4 OFF");

                    Thread.Sleep(lightTime * 10);
                }
            }
            finally
            {
                controller.ClosePin(pinch1);
                controller.ClosePin(pinch2);
                controller.ClosePin(pinch3);
                controller.ClosePin(pinch4);
                controller.ClosePin(pin);

            }




        }

        public static void Test03()
        {

            GpioController controller = new GpioController(PinNumberingScheme.Board);

            var lightTime = 500;

            var pinch1 = 15;
            var pinch2 = 13;
            var pinch3 = 11;
            var pinch4 = 7;


            //controller.ClosePin(pinch1);
            //controller.ClosePin(pinch2);
            //controller.ClosePin(pinch3);
            //controller.ClosePin(pinch4);



            
            Console.WriteLine("Test Relay 4 Channels");


            
            controller.OpenPin(pinch1, PinMode.Output);
            controller.SetPinMode(pinch1, PinMode.Output); //per impostare il pin deve essre prima "open"

            
            controller.OpenPin(pinch2, PinMode.Output);
            controller.SetPinMode(pinch2, PinMode.Output); //per impostare il pin deve essre prima "open"

            
            controller.OpenPin(pinch3, PinMode.Output);
            controller.SetPinMode(pinch3, PinMode.Output); //per impostare il pin deve essre prima "open"

                        
        

            try
            {
                while (true)
                {
                    Console.WriteLine("ch1");
                    controller.Write(pinch1, PinValue.High);
                    Thread.Sleep(lightTime);

                    Console.WriteLine("ch2");
                    controller.Write(pinch2, PinValue.High);
                    Thread.Sleep(lightTime);

                    Console.WriteLine("ch3");
                    controller.Write(pinch3, PinValue.High);
                    Thread.Sleep(lightTime);

                    Console.WriteLine("ch4");                   
                    controller.Write(pinch4, PinValue.High);  // funziona solo la prima volta 
                    Thread.Sleep(lightTime);



                    controller.Write(pinch1, PinValue.Low);
                    controller.Write(pinch2, PinValue.Low);
                    controller.Write(pinch3, PinValue.Low);
                    controller.Write(pinch4, PinValue.Low);
                    Thread.Sleep(lightTime*2);
                }
            }
            finally
            {
                controller.ClosePin(pinch1);
                controller.ClosePin(pinch2);
                controller.ClosePin(pinch3);
                controller.ClosePin(pinch4);

            }



        }


        public static void Test02()
        {
            GpioController controller = new GpioController(PinNumberingScheme.Board);

            var pin = 10;
            var lightTime = 500;
            //var Pin1wire = 7;
            string[] listaSonde = System.IO.Directory.GetDirectories("/sys/bus/w1/devices/", "28-*"); //   GetFiles("/sys/bus/w1/devices/","28-*");


            Console.WriteLine("Sonde trovate:");

            //System.IO.StreamReader[] sonda = new System.IO.StreamReader[listaSonde.GetLength(0)];
            for (int i = 0; i < listaSonde.GetLength(0); i++)
            {
                //sonda[i] = new System.IO.StreamReader(listaSonde[i]);
                Console.WriteLine(listaSonde[i]);
            }

            TempProbe sonda1 = new TempProbe("/sys/bus/w1/devices/" + "28-012033bfd29c", 0);
            TempProbe sonda2 = new TempProbe("/sys/bus/w1/devices/" + "28-012033aa416f", 0.5m);   // suffisso m per dichiarare 0.5 decimal al compilatore

           
            //System.IO.Stream sonda1b = new System.IO.Stream("/sys/bus/w1/devices/" + "28-" + "012033bfd29c" + "/temperature",);
            //System.IO.Stream sonda2b = new System.IO.Stream("/sys/bus/w1/devices/" + "28-" + "012033aa416f" + "/temperature");



            //System.IO.StringReader t1 = System.IO.File.OpenRead("/sys/bus/w1/devices/" + "28-" + "012033bfd29c" + "/w1_slave");
            //System.IO.FileStream t2 = System.IO.File.OpenRead("/sys/bus/w1/devices/" + "28-" + "012033aa416f" + "/w1_slave");

            string line;

            controller.OpenPin(pin, PinMode.Output);
            //
            //controller.SetPinMode(Pin1wire, PinMode.InputPullUp);

            Console.WriteLine("Inizio operazione: blink GPIO7 e lettura temperature");
            try
            {
                while (true)
                {
                    controller.Write(pin, PinValue.High);
                    Thread.Sleep(lightTime);

                    //devo reinizializzare la classe perchè dopo aver letto sono alla fine del file
                    //sonda1 = new System.IO.StreamReader("/sys/bus/w1/devices/" + "28-" + "012033bfd29c" + "/temperature");
                    //sonda2 = new System.IO.StreamReader("/sys/bus/w1/devices/" + "28-" + "012033aa416f" + "/temperature");
                    //utilizzo stream per poter leggere il file dall'inizio tutrte le volete che serve 

                    Console.WriteLine(DateTime.Now.ToString());
                    line = "rawtemp= " + sonda1.RawTemp().ToString() + "\t\t temp=" + sonda1.Temp().ToString();
                    Console.WriteLine("sonda1->" + line);

                    line = "rawtemp= " + sonda2.RawTemp().ToString() + "\t\t temp=" + sonda2.Temp().ToString();
                    Console.WriteLine("sonda2->" + line);

                    controller.Write(pin, PinValue.Low);
                    Thread.Sleep(lightTime);
                }
            }
            finally
            {
                controller.ClosePin(pin);
            }



        }

        public static  void Test01()
        {
            GpioController controller = new GpioController(PinNumberingScheme.Board);

            var pin = 10;
            var lightTime = 500;
            //var Pin1wire = 7;
            string[] listaSonde = System.IO.Directory.GetDirectories("/sys/bus/w1/devices/", "28-*"); //   GetFiles("/sys/bus/w1/devices/","28-*");


            Console.WriteLine("Sonde trovate:");

            //System.IO.StreamReader[] sonda = new System.IO.StreamReader[listaSonde.GetLength(0)];
            for (int i = 0; i < listaSonde.GetLength(0); i++)
            {
                //sonda[i] = new System.IO.StreamReader(listaSonde[i]);
                Console.WriteLine(listaSonde[i]);
            }

            System.IO.StreamReader sonda1 = new System.IO.StreamReader("/sys/bus/w1/devices/" + "28-" + "012033bfd29c" + "/temperature");
            System.IO.StreamReader sonda2 = new System.IO.StreamReader("/sys/bus/w1/devices/" + "28-" + "012033aa416f" + "/temperature");

            //System.IO.Stream sonda1b = new System.IO.Stream("/sys/bus/w1/devices/" + "28-" + "012033bfd29c" + "/temperature",);
            //System.IO.Stream sonda2b = new System.IO.Stream("/sys/bus/w1/devices/" + "28-" + "012033aa416f" + "/temperature");



            //System.IO.StringReader t1 = System.IO.File.OpenRead("/sys/bus/w1/devices/" + "28-" + "012033bfd29c" + "/w1_slave");
            //System.IO.FileStream t2 = System.IO.File.OpenRead("/sys/bus/w1/devices/" + "28-" + "012033aa416f" + "/w1_slave");

            string line;

            controller.OpenPin(pin, PinMode.Output);
            //
            //controller.SetPinMode(Pin1wire, PinMode.InputPullUp);

            Console.WriteLine("Inizio opreazione: blink GPIO7 e lettura temperature");
            try
            {
                while (true)
                {
                    controller.Write(pin, PinValue.High);
                    Thread.Sleep(lightTime);

                    //devo reinizializzare la classe perchè dopo aver letto sono alla fine del file
                    //sonda1 = new System.IO.StreamReader("/sys/bus/w1/devices/" + "28-" + "012033bfd29c" + "/temperature");
                    //sonda2 = new System.IO.StreamReader("/sys/bus/w1/devices/" + "28-" + "012033aa416f" + "/temperature");
                    //utilizzo stream per poter leggere il file dall'inizio tutrte le volete che serve 


                    line = sonda1.ReadLine();
                    //line += sonda1.ReadLine();
                    Console.WriteLine("sonda1=" + line);

                    line = sonda2.ReadLine();
                    //line += sonda2.ReadLine();
                    Console.WriteLine("sonda2=" + line);

                    controller.Write(pin, PinValue.Low);
                    Thread.Sleep(lightTime);

                    sonda1.BaseStream.Seek(0, System.IO.SeekOrigin.Begin);
                    sonda2.BaseStream.Seek(0, System.IO.SeekOrigin.Begin);
                    //Console.ReadLine();
                }
            }
            finally
            {
                controller.ClosePin(pin);
                sonda1.Close();
                sonda2.Close();
            }

        }
 public static Single BytesToSingle(byte[] Buffer, int StartIndex)
        {

            if ((Buffer.Length - StartIndex) < 4) return -1;

            byte[] _internal = new byte[4];
            _internal[3]= Buffer[StartIndex +0];
            _internal[2]= Buffer[StartIndex +1];
            _internal[1]= Buffer[StartIndex +2];
            _internal[0]= Buffer[StartIndex +3];

            return BitConverter.ToSingle(_internal);

        }


    }


}


/*
 
 import os
import glob
import time
 
os.system('modprobe w1-gpio')
os.system('modprobe w1-therm')
 
base_dir = '/sys/bus/w1/devices/'
device_folder = glob.glob(base_dir + '28*')[0]
device_file = device_folder + '/w1_slave'
 
def read_temp_raw():
    f = open(device_file, 'r')
    lines = f.readlines()
    f.close()
    return lines
 
def read_temp():
    lines = read_temp_raw()
    while lines[0].strip()[-3:] != 'YES':
        time.sleep(0.2)
        lines = read_temp_raw()
    equals_pos = lines[1].find('t=')
    if equals_pos != -1:
        temp_string = lines[1][equals_pos+2:]
        temp_c = float(temp_string) / 1000.0
        temp_f = temp_c * 9.0 / 5.0 + 32.0
        return temp_c, temp_f
	
while True:
	print(read_temp())	
	time.sleep(1)
 
 
 */