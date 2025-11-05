using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapaRiesgos_SCI
{
    public class SensorManager
    {
        public const int NUM_PISOS = 5; // Número total de pisos

        public const int HUMO = 0;
        public const int TEMPERATURA = 1;
        public const int MANUAL = 2;

        private Random rnd = new Random(); // Generador aleatorio

        // Genera lecturas aleatorias para cada piso
        public int[,] GenerarLecturas()
        {
            int[,] datos = new int[NUM_PISOS, 3]; // humo, temperatura, manual

            for (int p = 0; p < NUM_PISOS; p++)
            {
                datos[p, HUMO] = rnd.Next(0, 101); // humo %
                datos[p, TEMPERATURA] = rnd.Next(0, 101); // temperatura %
                datos[p, MANUAL] = (rnd.Next(0, 100) < 2) ? 1 : 0; // 2% de prob. manual
            }

            return datos;
        }

        // Calcula nivel de riesgo: 0=bajo, 1=medio, 2=alto
        public int[] CalcularRiesgo(int[,] datos)
        {
            int[] riesgo = new int[NUM_PISOS];

            for (int p = 0; p < NUM_PISOS; p++)
            {
                int humo = datos[p, 0];
                int temp = datos[p, 1];
                int manual = datos[p, 2];

                if (manual == 1) riesgo[p] = 2;
                else if (humo >= 80 || temp >= 57) riesgo[p] = 2;
                else if (humo >= 60 || temp >= 40) riesgo[p] = 1;
                else riesgo[p] = 0;
            }
            return riesgo;
        }

        // Detecta si hay incendio (retorna piso o -1)
        public int? DetectarIncendio(int[,] datos)
        {
            for (int p = 0; p < NUM_PISOS; p++)
            {
                int humo = datos[p, 0];
                int temp = datos[p, 1];
                int manual = datos[p, 2];

                if (manual == 1 || (humo >= 90 && temp >= 57))
                    return p + 1; // piso con incendio
            }
            return null; // sin incendio
        }
    }
}
