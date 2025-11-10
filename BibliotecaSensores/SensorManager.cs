using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibliotecaSensores
{
    public class SensorManager
    {
        public const int NUM_PISOS = 5;
        private Random rnd = new Random();

        // Umbrales y probabilidades 
        public readonly double UmbralTempIncendioC = 87.0;
        public readonly int UmbralTempIncendioPct = 97;
        public readonly int UmbralHumoIncendioPct = 90;
        public readonly int ProbManualPct = 2;

        // Genera lecturas simuladas (humo%, temp%, manual)
        public int[,] GenerarLecturas()
        {
            int[,] datos = new int[NUM_PISOS, 3];

            for (int p = 0; p < NUM_PISOS; p++)
            {
                // HUMO %
                double rh = rnd.NextDouble();
                if (rh < 0.85) datos[p, 0] = rnd.Next(0, 46);
                else if (rh < 0.97) datos[p, 0] = rnd.Next(46, 80);
                else datos[p, 0] = rnd.Next(UmbralHumoIncendioPct, 101);

                // TEMPERATURA %
                double rt = rnd.NextDouble();
                if (rt < 0.85) datos[p, 1] = rnd.Next(0, 61);
                else if (rt < 0.97) datos[p, 1] = rnd.Next(61, 97);
                else datos[p, 1] = rnd.Next(UmbralTempIncendioPct, 101);

                // MANUAL
                datos[p, 2] = (rnd.Next(0, 100) < ProbManualPct) ? 1 : 0;
            }

            return datos;
        }

        // Convierte porcentaje (0..100)% a °C en escala (0..90)°C
        public double PorcentajeATemperatura(int porcentaje)
        {
            return Math.Round((porcentaje / 100.0) * 90.0, 1);
        }

        // Calcula riesgo por piso (0=bajo,1=medio,2=alto/incendio)
        public int[] CalcularRiesgo(int[,] datos)
        {
            int[] riesgo = new int[NUM_PISOS];
            for (int p = 0; p < NUM_PISOS; p++)
            {
                int humo = datos[p, 0];
                int tempPct = datos[p, 1];
                int manual = datos[p, 2];
                double tempC = PorcentajeATemperatura(tempPct);

                if (manual == 1 || humo >= UmbralHumoIncendioPct || tempC >= UmbralTempIncendioC)
                    riesgo[p] = 2;
                else if (humo >= 60 || tempC >= 72.0)
                    riesgo[p] = 2;
                else if (humo >= 40 || tempC >= 57.0)
                    riesgo[p] = 1;
                else
                    riesgo[p] = 0;
            }
            return riesgo;
        }

        public int DetectarIncendio(int[,] datos)
        {
            for (int p = 0; p < NUM_PISOS; p++)
            {
                int humo = datos[p, 0];
                int tempPct = datos[p, 1];
                int manual = datos[p, 2];
                double tempC = PorcentajeATemperatura(tempPct);

                if (manual == 1 || humo >= UmbralHumoIncendioPct || tempC >= UmbralTempIncendioC)
                    return p + 1;
            }
            return -1;
        }
    }
}
