using System;
using System.IO.Ports;
using System.Threading.Tasks;
namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Arduino_Serivce
{
    public class ArduinoLog
    {
        private readonly SerialPort _serialPort;

        public ArduinoLog(string portName, int baudRate)
        {
            _serialPort = new SerialPort(portName, baudRate);
        }

        public async Task InitializeAsync()
        {
            try
            {
                _serialPort.Open();
                await Task.Delay(1000);
            }
            catch (Exception) { }
        }

        public async Task<string> ReadLineAsync()
        {
            try {
                await Task.Delay(1000);
                string result = _serialPort.ReadLine();
                return result.Substring(0,result.Length - 1);
            }
            catch (Exception) { }
            return string.Empty;
        }

        public async Task WriteLineAsync(string data)
        {
            try {
                await Task.Delay(1000);
                _serialPort.WriteLine(data);
            }
            catch (Exception) { }
        }

        public void Dispose()
        {
            _serialPort?.Close();
        }
    }
}
