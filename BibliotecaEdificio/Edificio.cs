using BibliotecaSensores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BibliotecaEdificio
{
    public class Edificio
    {
        private const int ANCHO = 42;
        private List<string> historial = new List<string>();

        // --- SIMULACIÓN AUTOMÁTICA ---
        public void SimulacionAutomatica(SensorManager s, int intervalo)
        {
            bool continuar = true;
            while (continuar)
            {
                Console.Clear();
                int[,] datos = s.GenerarLecturas();
                int[] riesgo = s.CalcularRiesgo(datos);
                int pisoIncendio = s.DetectarIncendio(datos);

           
                DibujarMapa(s, datos, riesgo, pisoIncendio);
                MostrarDetallesPorPiso(s, datos, riesgo);
                GuardarHistorial(s, datos, riesgo, pisoIncendio);

                if (pisoIncendio != -1)
                {
                    string motivo = ObtenerMotivoIncendio(s, datos, pisoIncendio);
                    ProtocoloIncendio(pisoIncendio, s, datos, riesgo, motivo);
                    return;
                }

                Console.WriteLine("\nPresione 'E' para detener o espere la siguiente verificación...");
                int elapsed = 0;
                while (elapsed < intervalo)
                {
                    if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.E)
                    {
                        continuar = false;
                        break;
                    }
                    Thread.Sleep(500);
                    elapsed += 500;
                }
            }
        }

        // --- VERIFICACIÓN POR PISO ---
        public void VerificarPorPiso(SensorManager s)
        {
            Console.Write("\nIngrese número de piso (1-" + SensorManager.NUM_PISOS + "): ");
            if (!int.TryParse(Console.ReadLine(), out int piso) || piso < 1 || piso > SensorManager.NUM_PISOS)
            {
                Console.WriteLine("Número inválido.");
                Thread.Sleep(800);
                return;
            }

            int[,] datos = s.GenerarLecturas();
            int[] riesgo = s.CalcularRiesgo(datos);
            Console.Clear();
            DibujarPiso(s, piso, datos, riesgo);

            int pisoIncendio = s.DetectarIncendio(datos);
            if (pisoIncendio == piso)
            {
                string motivo = ObtenerMotivoIncendio(s, datos, piso);
                ProtocoloIncendio(piso, s, datos, riesgo, motivo);
            }
            else
            {
                Console.WriteLine("\nPresione una tecla para volver...");
                Console.ReadKey(true);
            }
        }

        // --- ACTIVACIÓN MANUAL ---
        public void ActivarIncendioManual(SensorManager s)
        {
            Console.Write("\nIngrese piso para activación manual (1-" + SensorManager.NUM_PISOS + "): ");
            if (!int.TryParse(Console.ReadLine(), out int piso) || piso < 1 || piso > SensorManager.NUM_PISOS)
            {
                Console.WriteLine("Piso inválido.");
                Thread.Sleep(800);
                return;
            }

            int[,] datos = s.GenerarLecturas();
            datos[piso - 1, 2] = 1; // activar manualmente
            int[] riesgo = s.CalcularRiesgo(datos);
            Console.Clear();
            DibujarPiso(s, piso, datos, riesgo);

            string motivo = "Activación manual del sistema por el usuario.";
            ProtocoloIncendio(piso, s, datos, riesgo, motivo);
        }

        // --- DIBUJAR MAPA ---
        private void DibujarMapa(SensorManager s, int[,] datos, int[] riesgo, int pisoIncendio)

        {
  
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔════════════ MAPA DE RIESGOS ════════════╗");
            Console.ResetColor();

            for (int p = SensorManager.NUM_PISOS; p >= 1; p--)
            {
                int idx = p - 1;
                string etiqueta = $" PISO {p} ";
                ConsoleColor bg = ConsoleColor.Black;
                ConsoleColor fg = ConsoleColor.White;

                if (riesgo[idx] == 0) { bg = ConsoleColor.DarkGreen; etiqueta += " - BAJO "; }
                else if (riesgo[idx] == 1) { bg = ConsoleColor.Yellow; fg = ConsoleColor.Black; etiqueta += " - MEDIO "; }
                else if (riesgo[idx] == 2) { bg = ConsoleColor.DarkYellow ; etiqueta += " - ALTO "; }

                if (p == pisoIncendio)
                {
                    bg = ConsoleColor.Red;
                    etiqueta = $" ⚠ PISO {p} - ¡INCENDIO! ⚠ ";
                }

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("┌" + new string('─', ANCHO - 2) + "┐");
                Console.BackgroundColor = bg;
                Console.ForegroundColor = fg;
                Console.WriteLine("│" + Centrar(etiqueta, ANCHO - 2) + "│");
                Console.ResetColor();

                double tempC = s.PorcentajeATemperatura(datos[idx, 1]);
                string info = $" Humo:{datos[idx, 0],3}%  Temp:{tempC,5}°C  Manual:{(datos[idx, 2] == 1 ? "Sí" : "No")}";
                Console.WriteLine("│" + info.PadRight(ANCHO - 2) + "│");
                Console.WriteLine("└" + new string('─', ANCHO - 2) + "┘");
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╚═════════════════════════════════════════╝");
            Console.ResetColor();
        }

        private void DibujarPiso(SensorManager s, int piso, int[,] datos, int[] riesgo)
        {
            int idx = piso - 1;
            double tempC = s.PorcentajeATemperatura(datos[idx, 1]);
            Console.WriteLine($"\nDETALLE PISO {piso}:");
            Console.WriteLine($" Humo: {datos[idx, 0]}%");
            Console.WriteLine($" Temperatura: {tempC:0.0}°C");
            Console.WriteLine($" Nivel: {(riesgo[idx] == 0 ? "BAJO" : riesgo[idx] == 1 ? "MEDIO" : "ALTO")}");
        }

        private void MostrarDetallesPorPiso(SensorManager s, int[,] datos, int[] riesgo)
        {
            Console.WriteLine("\nDETALLES POR PISO:");
            for (int p = 0; p < SensorManager.NUM_PISOS; p++)
            {
                double tempC = s.PorcentajeATemperatura(datos[p, 1]);
                string manual = (datos[p, 2] == 1) ? "Sí" : "No";
                string nivel = riesgo[p] == 0 ? "BAJO" : riesgo[p] == 1 ? "MEDIO" : "ALTO";
                Console.WriteLine($" Piso {p + 1}: Humo={datos[p, 0],3}%  Temp={tempC,5}°C  Manual={manual}  Nivel={nivel}");
            }
        }

        // --- MOTIVO Y UMBRAL DEL INCENDIO ---
        private string ObtenerMotivoIncendio(SensorManager s, int[,] datos, int piso)
        {
            int idx = piso - 1;
            int humo = datos[idx, 0];
            double tempC = s.PorcentajeATemperatura(datos[idx, 1]);
            int manual = datos[idx, 2];

            string motivo;
            if (manual == 1)
                motivo = "Activación manual del sistema por el usuario.";
            else if (humo >= s.UmbralHumoIncendioPct && tempC >= s.UmbralTempIncendioC)
                motivo = $"Incendio detectado por alta temperatura ({tempC:0.0}°C) y concentración de humo ({humo}%).";
            else if (humo >= s.UmbralHumoIncendioPct)
                motivo = $"Incendio detectado por concentración de humo elevada ({humo}%).";
            else if (tempC >= s.UmbralTempIncendioC)
                motivo = $"Incendio detectado por temperatura crítica ({tempC:0.0}°C).";
            else
                motivo = "Incendio detectado por combinación de sensores.";

            // Agregar los umbrales configurados del sistema
            motivo += $"\n   (Umbral de temperatura: {s.UmbralTempIncendioC}°C / {s.UmbralTempIncendioPct}%)";
            motivo += $"\n   (Umbral de humo: {s.UmbralHumoIncendioPct}%)";
            return motivo;
        }

        // --- PROTOCOLO DE INCENDIO ---
        private void ProtocoloIncendio(int piso, SensorManager s, int[,] datos, int[] riesgo, string motivo)
        {
            Console.Clear();
            DibujarMapa(s, datos, riesgo, piso);

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n⚠ ALERTA DE INCENDIO EN EL PISO {piso} ⚠");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"\n🔍 MOTIVO: {motivo}");
            Console.ResetColor();

            var cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;

            Task alarma = Task.Run(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        Console.Beep(1200, 700);
                        Console.Beep(1400, 700);
                    }
                    catch { }
                }
            }, token);

            Console.WriteLine("\n📞 Llamando automáticamente a los bomberos...");
            Thread.Sleep(1200);
            Console.WriteLine("🚒 Bomberos en camino.\n");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=== GUÍA DE EVACUACIÓN ===");
            Console.ResetColor();

            string[] pasos =
            {
                "1) Mantenga la calma.",
                "2) Diríjase a la salida más cercana.",
                "3) No use ascensores.",
                "4) Siga las luces de emergencia.",
                "5) Reúnase en el punto de encuentro."
            };
            foreach (string paso in pasos)
            {
                Console.WriteLine(paso);
                Thread.Sleep(900);
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n🚨 Presione cualquier tecla para detener la alarma y volver al menú...");
            Console.ResetColor();

            Console.ReadKey(true);
            cts.Cancel();
            try { alarma.Wait(500); } catch { }
        }

        // --- HISTORIAL ---
        private void GuardarHistorial(SensorManager s, int[,] datos, int[] riesgo, int pisoIncendio)
        {
            string entrada = $"{DateTime.Now:dd/MM/yyyy HH:mm:ss} | ";
            for (int p = 0; p < SensorManager.NUM_PISOS; p++)
            {
                double tempC = s.PorcentajeATemperatura(datos[p, 1]);
                string nivel = riesgo[p] == 0 ? "BAJO" : riesgo[p] == 1 ? "MEDIO" : "ALTO";
                entrada += $"P{p + 1,-2} H:{datos[p, 0],3}%  T:{tempC,5}°C  N:{nivel,-5} | ";
            }
            if (pisoIncendio != -1)
            {
                string motivo = ObtenerMotivoIncendio(s, datos, pisoIncendio);
                entrada += $"🔥 INCENDIO EN PISO {pisoIncendio} ({motivo.Replace("\n", " ")})";
            }
            historial.Add(entrada);
        }

        public void MostrarHistorial()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(Centrar("╔══════════════════════════════════════════════════════════════════════════════╗", 90));
            Console.WriteLine(Centrar("║              📋 HISTORIAL DE LECTURAS DEL SISTEMA DE INCENDIO               ║", 90));
            Console.WriteLine(Centrar("╚══════════════════════════════════════════════════════════════════════════════╝", 90));
            Console.ResetColor();
            Console.WriteLine();

            if (!historial.Any())
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine(Centrar("No hay registros aún. Realice una verificación primero.", 90));
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Fecha/Hora".PadRight(25) + "|  Registro de eventos");
                Console.ResetColor();
                Console.WriteLine(new string('-', 90));

                // Recorremos el historial desde el más reciente al más antiguo
                for (int i = historial.Count - 1; i >= 0; i--)
                {
                    string registro = historial[i];

                    // Si el registro contiene "INCENDIO", lo mostramos en rojo
                    if (registro.Contains("INCENDIO"))
                        Console.ForegroundColor = ConsoleColor.Red;
                    else
                        Console.ForegroundColor = ConsoleColor.Gray;

                    Console.WriteLine($" {registro}");
                }
            }

            Console.ResetColor();
            Console.WriteLine("\n" + new string('═', 90));
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(Centrar("Presione cualquier tecla para volver al menú principal...", 90));
            Console.ResetColor();
            Console.ReadKey(true);
        }

        private string Centrar(string texto, int ancho)
        {
            if (texto.Length >= ancho) return texto.Substring(0, ancho);
            int espacio = ancho - texto.Length;
            int izq = espacio / 2;
            return new string(' ', izq) + texto + new string(' ', espacio - izq);
        }
    }
}
