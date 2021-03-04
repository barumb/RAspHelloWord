using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaspHelloWord
{
    public class TempProbe
    {
        private decimal _offset;
        private string _id;
        private System.IO.StreamReader _device;

        public string Id { get => _id;  }

        public TempProbe(string ID, decimal offset=0)
        {
            _id = ID;
            _offset = offset;
            _device = new System.IO.StreamReader( _id + "/temperature");
        }

        private decimal _GetTemp()
        {
            decimal t = 200.000m;
            string line;
            _device.BaseStream.Seek(0, System.IO.SeekOrigin.Begin);
            line = _device.ReadLine();
            Decimal.TryParse(line, out t);
            t = t / 1000;

            return t;
        }
         
        public decimal RawTemp()
        {
            return _GetTemp();
        }

        public decimal Temp()
        {
            return _GetTemp() + _offset;
        }

        public void SetCfg()
        { }



    }
}
