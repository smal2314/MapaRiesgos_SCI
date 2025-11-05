using System;
using System.Threading;

namespace MapaRiesgos_SCI
{
    // Clase principal: controla el flujo general y muestra el menú
    internal class Program
    {
        // Intervalo en milisegundos para la simulación automática (10 segundos)
        static int intervalo = 10000;

        static void Main(string[] args)
        {
            Console.Title = "MAPA DE RIESGOS - SISTEMA CONTRA INCENDIOS";
            //Console.OutputEncoding = System.Text.Encoding.UTF8;//configuracion de sistema de codificación de caracteres
           

            // Crear objetos de las otras clases (bibliotecas)

            SensorManager sensorManager = new SensorManager();
            Edificio edificio = new Edificio();

            //declaracion e inicializacion de variables

            bool salir = false;

            while (!salir)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("=== SISTEMA CONTRA INCENDIOS ===");
                Console.ResetColor();
                Console.WriteLine("[1] Verificar sistema completo (auto cada 10s)");
                Console.WriteLine("[2] Verificar por piso");
                Console.WriteLine("[3] Activación manual de incendio");
                Console.WriteLine("[4] Mostrar historial de lecturas");
                Console.WriteLine("[5] Configurar intervalo de actualización");
                Console.WriteLine("[6] Salir");
                Console.Write("\nSeleccione una opción: ");

                string opcion = Console.ReadLine();

                switch (opcion)
                {
                    case "1":
                        edificio.SimulacionAutomatica(sensorManager, intervalo);
                        break;
                    case "2":
                        edificio.VerificarPorPiso(sensorManager);
                        break;
                    case "3":
                        edificio.ActivarIncendioManual();
                        break;
                    case "4":
                        edificio.MostrarHistorial();
                        break;
                    case "5":
                        Console.Write("\nNuevo intervalo (segundos): ");
                        if (int.TryParse(Console.ReadLine(), out int segs) && segs > 0)
                        {
                            intervalo = segs * 1000;
                            Console.WriteLine($"Intervalo actualizado a {segs} segundos.");
                        }
                        else
                        {
                            Console.WriteLine("Valor inválido, se mantiene igual.");
                        }
                        Thread.Sleep(1000);
                        break;
                    case "6":
                        salir = true;
                        break;
                    default:
                        Console.WriteLine("Opción no válida.");
                        Thread.Sleep(1000);
                        break;
                }
            }

            Console.WriteLine("\nPrograma finalizado. Presione una tecla para cerrar.");
            Console.ReadKey(true);
        }
    }
}
