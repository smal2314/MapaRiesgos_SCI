using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;


using BibliotecaSensores;
using BibliotecaEdificio;

namespace MapaRiesgos_SCI
{
    internal class Program
    {
        public static int intervalo = 5000; // Intervalo de actualización en milisegundos
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.Title = "MAPA DE RIESGOS - SISTEMA CONTRA INCENDIOS (SIMULADOR)";
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            SensorManager sensorManager = new SensorManager();
            Edificio edificio = new Edificio();

            bool salir = false;
            while (!salir)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("╔════════════════════════════════════════════════╗");
                Console.WriteLine("║   SISTEMA DE DETECCIÓN Y CONTROL DE INCENDIOS  ║");
                Console.WriteLine("╚════════════════════════════════════════════════╝");
                Console.ResetColor();

                Console.WriteLine("\nSeleccione una opción:");
                Console.WriteLine(" 1. 🔁 Simulación automática del edificio");
                Console.WriteLine(" 2. 🔍 Verificar por piso");
                Console.WriteLine(" 3. 🚨 Activación manual por piso");
                Console.WriteLine(" 4. ⚙️ Ver configuración de umbrales del sistema");
                Console.WriteLine(" 5. 📋 Ver historial de lecturas");
                Console.WriteLine(" 6. ⏱️ Configurar intervalo de actualización");

                Console.WriteLine(" E. ❌ Salir");
                Console.Write("\nOpción: ");

                string opcion = Console.ReadLine()?.ToUpper();

                switch (opcion)
                {
                    case "1":
                        edificio.SimulacionAutomatica(sensorManager, 5000); // cada 5 segundos
                        break;

                    case "2":
                        edificio.VerificarPorPiso(sensorManager);
                        break;

                    case "3":
                        edificio.ActivarIncendioManual(sensorManager);
                        break;

                    case "4":
                        MostrarUmbrales(sensorManager);
                        break;

                    case "5":
                        edificio.MostrarHistorial();
                        break;

                    case "6":
                        Console.Write("\nNuevo intervalo (segundos): ");
                        if (int.TryParse(Console.ReadLine(), out int segs) && segs > 0)
                        {
                            intervalo = segs * 1000;
                            Console.WriteLine("Intervalo actualizado a " + segs + " segundos.");
                            Thread.Sleep(1000);
                        }
                        break;

                    case "E":
                        salir = true;
                        break;

                    default:
                        Console.WriteLine("Opción inválida. Intente nuevamente.");
                        Thread.Sleep(1000);
                        break;
                }
            }

            Console.WriteLine("\nSimulación finalizada. Gracias por usar el sistema.");
            Thread.Sleep(1500);
        }

        //  Mostrar configuración de umbrales del sistema ---
        static void MostrarUmbrales(SensorManager s)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=== CONFIGURACIÓN ACTUAL DE UMBRALES ===\n");
            Console.ResetColor();

            Console.WriteLine($" 🔥 Temperatura crítica (incendio): {s.UmbralTempIncendioC} °C ({s.UmbralTempIncendioPct}%)");
            Console.WriteLine($" 🌫️  Humo crítico (incendio): {s.UmbralHumoIncendioPct}%");
            Console.WriteLine($" 🧯 Probabilidad de activación manual: {s.ProbManualPct}%");

            Console.WriteLine("\nPresione cualquier tecla para volver al menú...");
            Console.ReadKey(true);
        }
    }
}
