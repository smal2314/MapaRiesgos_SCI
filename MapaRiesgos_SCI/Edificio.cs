using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MapaRiesgos_SCI
{
    class Edificio
    {
        private const int ANCHO = 34;
        private List<string> historial = new List<string>();

        // Simulacion automática cada cierto tiempo
        public void SimulacionAutomatica(SensorManager s, int duracionAlarma)
        {
            bool continuar = true;

            while (continuar)
            {
                Console.Clear();

                int[,] datos = s.GenerarLecturas();
                int[] riesgos = s.CalcularRiesgo(datos);
                int? pisoIncendio = s.DetectarIncendio(datos);

                GuardarHistorial(datos, riesgos, pisoIncendio ?? -1);

                DibujarMapa(datos, riesgos, pisoIncendio ?? -1);

                if (pisoIncendio.HasValue)
                {
                    ProtocoloIncendio(pisoIncendio.Value);
                }

                Console.WriteLine("\nPresione Q para detener o espere...");

                int transcurrido = 0;
                while (transcurrido < duracionAlarma)
                {
                    if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Q)
                    {
                        continuar = false;
                        break;
                    }
                    Thread.Sleep(100);
                    transcurrido += 100;
                }
            }
        }

        private void ProtocoloIncendio(int piso)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nALERTA: INCENDIO EN PISO " + piso);
            Console.ResetColor();

            for (int i = 0; i < 3; i++)
            {
                // Manejamos excepciones que puedan ocurrir en el Console.Beep
                // Para que el programa no falle en sistemas que no soporten sonido
                try
                {
                    Console.Beep(1000, 200);
                    Console.Beep(1400, 200);
                }
                catch { }
                Thread.Sleep(300);
            }

            Console.WriteLine("\nEvacuación en curso...");
            Thread.Sleep(3000);
        }

        // Verifica un piso específico
        public void VerificarPorPiso(SensorManager s)
        {
            Console.Write("\nIngrese número de piso (1-" + SensorManager.NUM_PISOS + "): ");
            if (int.TryParse(Console.ReadLine(), out int piso) && piso >= 1 && piso <= SensorManager.NUM_PISOS)
            {
                int[,] datos = s.GenerarLecturas();
                int[] riesgo = s.CalcularRiesgo(datos);
                Console.Clear();
                DibujarPiso(piso, datos, riesgo);
            }
            else
            {
                Console.WriteLine("Número inválido.");
                Console.ReadKey(true);
            }
        }

        // Muestra datos de un piso
        private void DibujarPiso(int piso, int[,] datos, int[] riesgo)
        {
            int idx = piso - 1;
            Console.WriteLine($"\nPISO {piso}");
            Console.WriteLine($"Humo: {datos[idx, 0]}% | Temperatura: {datos[idx, 1]}% | Manual: {(datos[idx, 2] == 1 ? "PRESIONADO" : "OK")}");
            Console.WriteLine($"Nivel de riesgo: {(riesgo[idx] == 0 ? "BAJO" : riesgo[idx] == 1 ? "MEDIO" : "ALTO")}");
            Console.WriteLine("\nPresione una tecla para volver...");
            Console.ReadKey(true);
        }

        // Activa un incendio manualmente
        public void ActivarIncendioManual()
        {
            Console.Write("\nIngrese piso para activar incendio (1-" + SensorManager.NUM_PISOS + "): ");
            if (int.TryParse(Console.ReadLine(), out int piso) && piso >= 1 && piso <= SensorManager.NUM_PISOS)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nIncendio manual activado en el Piso {piso}");
                Console.ResetColor();
                ProtocoloIncendio(piso);
            }
            else
            {
                Console.WriteLine("Piso inválido.");
                Console.ReadKey(true);
            }
        }

        // Dibuja el edificio con colores
        private void DibujarMapa(int[,] datos, int[] riesgo, int pisoIncendio)
        {
            Console.WriteLine("MAPA DE RIESGOS - EDIFICIO (Piso 5 arriba)");
            Console.WriteLine(new string('-', ANCHO));

            for (int p = SensorManager.NUM_PISOS; p >= 1; p--)
            {
                int idx = p - 1;
                string etiqueta = $" PISO {p} ";
                ConsoleColor bg = ConsoleColor.Black;
                ConsoleColor fg = ConsoleColor.White;

                if (riesgo[idx] == 0) { bg = ConsoleColor.DarkGreen; etiqueta += "- BAJO"; }
                else if (riesgo[idx] == 1) { bg = ConsoleColor.DarkYellow; fg = ConsoleColor.Black; etiqueta += "- MEDIO"; }
                else { bg = ConsoleColor.Red; etiqueta += "- ALTO"; }

                if (p == pisoIncendio)
                {
                    bg = ConsoleColor.DarkRed;
                    fg = ConsoleColor.Black;
                    etiqueta = $"PISO {p} - ¡INCENDIO!";
                }

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("┌" + new string('─', ANCHO - 2) + "┐");

                Console.BackgroundColor = bg;
                Console.ForegroundColor = fg;
                Console.WriteLine("│" + Centrar(etiqueta, ANCHO - 2) + "│");
                Console.ResetColor();

                string info = $" H:{datos[idx, 0],3}%  T:{datos[idx, 1],3}%  M:{(datos[idx, 2] == 1 ? "PRES" : " OK ")}";
                Console.WriteLine("│" + info.PadRight(ANCHO - 2) + "│");

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("└" + new string('─', ANCHO - 2) + "┘");
            }
        }

        // Centra un texto en un ancho dado
        private string Centrar(string texto, int ancho)
        {
            if (texto.Length >= ancho) return texto.Substring(0, ancho);
            int espacio = ancho - texto.Length;
            int izq = espacio / 2;
            return new string(' ', izq) + texto + new string(' ', espacio - izq);
        }

        // Muestra resumen de todos los pisos
        private void MostrarResumen(int[,] datos, int[] riesgo, int pisoIncendio)
        {
            Console.WriteLine("\nRESUMEN DE PISOS:");
            for (int p = 0; p < SensorManager.NUM_PISOS; p++)
            {
                string nivel = riesgo[p] == 0 ? "BAJO" : riesgo[p] == 1 ? "MEDIO" : "ALTO";
                Console.WriteLine($" Piso {p + 1}: Nivel={nivel} | Humo={datos[p, 0]}% | Temp={datos[p, 1]}% | Manual={(datos[p, 2] == 1 ? "PRESIONADO" : "OK")}{(p + 1 == pisoIncendio ? " 🔥" : "")}");
            }
        }

        // Guarda el historial de lecturas
        private void GuardarHistorial(int[,] datos, int[] riesgo, int pisoIncendio)
        {
            string entrada = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss") + " -> ";
            
            for (int p = 0; p < SensorManager.NUM_PISOS; p++)
                entrada += $"[P{p + 1}:H={datos[p, 0]}% T={datos[p, 1]}% R={riesgo[p]}] ";
            if (pisoIncendio != -1)
                entrada += $"PISO {pisoIncendio}";
            historial.Add(entrada);
        }

        // Muestra el historial de lecturas
        public void MostrarHistorial()
        {
            Console.Clear();
            Console.WriteLine("=== HISTORIAL ===\n");
            if (historial.Count == 0)
                Console.WriteLine("No hay registros aún.");
            else
                foreach (var h in historial)
                    Console.WriteLine(h);
            Console.WriteLine("\nPresione una tecla...");
            Console.ReadKey(true);
        }

    }
}
