using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mymtec.Core
{
    class Program
    {
        public class BloqueTiempo
        {
            public string Inicio { get; set; }
            public string Fin { get; set; }
        }
        public class Persona
        {
            public string Nombre { get; set; }
            public BloqueTiempo JornadaLaboral { get; set; }
            public List<BloqueTiempo> Actividades { get; set; }
        }

        static List<Persona> GetPersonas()
        {
            List<Persona> Personas = new List<Persona>
            {
                new Persona
                {
                    Nombre = "Carla",
                    JornadaLaboral = new BloqueTiempo { Inicio = "0900", Fin = "1800" },
                    Actividades = new List<BloqueTiempo>
                    {
                        new BloqueTiempo{ Inicio = "0900", Fin = "1130" },
                        new BloqueTiempo{ Inicio = "1300", Fin = "1400" },
                        new BloqueTiempo{ Inicio = "1500", Fin = "1540" }
                    }
                },
                new Persona
                {
                    Nombre = "José",
                    JornadaLaboral = new BloqueTiempo { Inicio = "0830", Fin = "1730" },
                    Actividades = new List<BloqueTiempo>
                    {
                        new BloqueTiempo{ Inicio = "1200", Fin = "1400" },
                        new BloqueTiempo{ Inicio = "1645", Fin = "1730" }
                    }
                },
                new Persona
                {
                    Nombre = "Maria",
                    JornadaLaboral = new BloqueTiempo { Inicio = "1000", Fin = "1700" },
                    Actividades = new List<BloqueTiempo>
                    {
                        new BloqueTiempo{ Inicio = "1000", Fin = "1100" },
                        new BloqueTiempo{ Inicio = "1530", Fin = "1600" }
                    }
                }
            };

            /* HORARIOS DISPONIBLES
             * POS 0 - 1130 - 1300 / 1400 - 1500 / 1540 - 1800
             * POS 1 - 0830 - 1200 / 1400 - 1645
             * POS 2 - 1100 - 1530 / 1600 - 1700
             */

            /*HORARIOS MATCH DE 1 HORA
             * 1400 - 1500 / 1400 - 1645 / 1100 - 1530
             */

            /*HORARIOS MATCH DE 30 HORA
             * 1130 - 1300 / 0830 - 1200 / 1100 - 1530
             * 1400 - 1500 / 1400 - 1645 / 1100 - 1530
             * 1540 - 1800 / 1400 - 1645 / 1600 - 1700
             */

            return Personas;
        }

        static void Main(string[] args)
        {
            List<Persona> Personas = GetPersonas();
            List<BloqueTiempo> HorariosPosibles = DevolverHorariosPosibles(Personas, 30);

            foreach (var item in HorariosPosibles)
            {
                Console.WriteLine("[{" + item.Inicio + "},{" + item.Fin + "}]");
            }

            Console.ReadKey();
        }

        static List<BloqueTiempo> DevolverHorariosPosibles(List<Persona> Personas, double Duracion)
        {
            //BUSCA LA DISPONIBILIDAD DE CADA UNA DE LAS PERSONAS.
            List<List<BloqueTiempo>> Diponibilidades = GetTiempoLibrePersonas(Personas, Duracion);

            //BUSCA LOS HORARIOS EN COMUN.
            List<BloqueTiempo> MatchHorarios = GetMatchHorarios(Diponibilidades, Duracion);

            //AGRUPA LOS HORARIOS EN COMUN, EN SUBLISTAS.
            List<List<BloqueTiempo>> HorariosAgrupados = AgruparHorarios(MatchHorarios, Personas.Count);

            //DEVUELVE PARA CADA UNA DE LAS SUBLISTAS UN HORARIO CON SU TIEMPO LIMITE. 
            List<BloqueTiempo> MatchConLimites = GetTiemposLimites(HorariosAgrupados, Duracion);

            return MatchConLimites;
        }

        static List<List<BloqueTiempo>> AgruparHorarios(List<BloqueTiempo> DispPersonas, int CantPersonas)
        {
            List<List<BloqueTiempo>> SubListas = new List<List<BloqueTiempo>>();
            List<BloqueTiempo> SubLista = new List<BloqueTiempo>(CantPersonas);

            int SubListaIndex = 0;
            foreach (var item in DispPersonas)
            {
                if (SubLista.Count == SubLista.Capacity)
                {
                    SubListas.Add(SubLista);
                    SubLista = new List<BloqueTiempo>(CantPersonas);
                    SubListaIndex++;
                }
                SubLista.Add(item);
            }

            if (SubLista.Count > 0) SubListas.Add(SubLista);

            return SubListas;
        }

        static List<BloqueTiempo> GetTiemposLimites(List<List<BloqueTiempo>> MatchHorarios, double Duracion)
        {
            List<BloqueTiempo> HorariosLimites = new List<BloqueTiempo>();

            if (MatchHorarios.All(SubListas => SubListas.Count == 1))
            {
                List<BloqueTiempo> UnicaLista = MatchHorarios
                    .Where(Horarios => Horarios.Count == 1)
                    .SelectMany(Horarios => Horarios)
                    .ToList();

                HorariosLimites.Add(BuscarHorarioLimite(UnicaLista, Duracion));
            }
            else
            {
                foreach (var Horarios in MatchHorarios)
                {
                    HorariosLimites.Add(BuscarHorarioLimite(Horarios, Duracion));
                }
            }

            return HorariosLimites;
        }

        static BloqueTiempo BuscarHorarioLimite(List<BloqueTiempo> Horarios, double Duracion)
        {
            TimeSpan InicialAux = new TimeSpan();
            TimeSpan FinAux = new TimeSpan();

            foreach (var item in Horarios)
            {
                TimeSpan Inicial = GetHorarioTimeSpan(item.Inicio);
                TimeSpan Fin = GetHorarioTimeSpan(item.Fin);

                if (InicialAux.TotalMinutes == 0 || Inicial > InicialAux) InicialAux = Inicial;
                if (FinAux.TotalMinutes == 0 || Fin < FinAux) FinAux = Fin;
            }

            double Diferencia = FinAux.Subtract(InicialAux).TotalMinutes;
            if (Diferencia == Duracion) FinAux = InicialAux;
            else FinAux = TimeSpan.FromMinutes(FinAux.TotalMinutes - Duracion);

            return new BloqueTiempo { Inicio = InicialAux.ToString(), Fin = FinAux.ToString() };
        }

        static List<BloqueTiempo> GetMatchHorarios(List<List<BloqueTiempo>> DispPersonas, double Duracion)
        {
            List<BloqueTiempo> DispMatch = new List<BloqueTiempo>();
            foreach (var Horario in DispPersonas[0])
            {
                List<BloqueTiempo> AuxList = new List<BloqueTiempo> { Horario };

                foreach (var DispPersona in DispPersonas.Skip(1))
                {
                    AuxList = CompararHorarios(DispPersona, AuxList, Duracion);
                }

                if (AuxList.Count >= DispPersonas.Count) DispMatch.AddRange(AuxList);
            }

            return DispMatch;
        }

        static List<BloqueTiempo> CompararHorarios(List<BloqueTiempo> DispPersona, List<BloqueTiempo> DispMatch, double Duracion)
        {
            foreach (var item in DispPersona)
            {
                TimeSpan AxI = GetHorarioTimeSpan(item.Inicio);
                TimeSpan AxF = GetHorarioTimeSpan(item.Fin);

                int Count = 0;
                foreach (var Horario in DispMatch)
                {
                    TimeSpan I = GetHorarioTimeSpan(Horario.Inicio);
                    TimeSpan F = GetHorarioTimeSpan(Horario.Fin);

                    if (I <= AxI && F <= AxF)
                    {
                        if (F.Subtract(AxI).TotalMinutes >= Duracion)
                        {
                            Count++;
                        }
                    }
                    else if (I >= AxI && F >= AxF)
                    {
                        if (AxF.Subtract(I).TotalMinutes >= Duracion)
                        {
                            Count++;
                        }
                    }
                    else if (I >= AxI && F <= AxF)
                    {
                        Count++;
                    }
                    else if (I <= AxI && F >= AxF)
                    {
                        Count++;
                    }
                }

                if (Count == DispMatch.Count) DispMatch.Add(item);
            }

            return DispMatch;
        }

        static List<List<BloqueTiempo>> GetTiempoLibrePersonas(List<Persona> Personas, double Duracion)
        {
            List<List<BloqueTiempo>> DiponibilidadesPersonas = new List<List<BloqueTiempo>>();

            foreach (var Persona in Personas)
            {
                List<BloqueTiempo> DisponibilidadPersona = new List<BloqueTiempo>();
                DisponibilidadPersona = GetHorariosDisponibles(Persona, Duracion);

                if (DisponibilidadPersona.Count > 0) DiponibilidadesPersonas.Add(DisponibilidadPersona);
            }

            return DiponibilidadesPersonas;
        }

        static TimeSpan GetHorarioTimeSpan(string Horario)
        {
            int Horas = Convert.ToInt32(Horario.Substring(0, 2));
            int Minutos = Convert.ToInt32(Horario.Substring(2, 2));

            return new TimeSpan(Horas, Minutos, 0);
        }

        static bool VerificarIntervalo(string Inicio, string Fin, double Duracion)
        {
            TimeSpan HorarioInicio = GetHorarioTimeSpan(Inicio);
            TimeSpan HorarioFin = GetHorarioTimeSpan(Fin);

            return (HorarioFin.Subtract(HorarioInicio)).TotalMinutes >= Duracion;
        }

        static List<BloqueTiempo> GetHorariosDisponibles(Persona Persona, double Duracion)
        {
            List<BloqueTiempo> HorariosDisponibles = new List<BloqueTiempo>();

            if (VerificarIntervalo(Persona.JornadaLaboral.Inicio, Persona.Actividades[0].Inicio, Duracion))
            {
                HorariosDisponibles.Add(new BloqueTiempo
                {
                    Inicio = Persona.JornadaLaboral.Inicio,
                    Fin = Persona.Actividades[0].Inicio
                });
            }

            for (int i = 0; i < Persona.Actividades.Count - 1; i++)
            {
                if (VerificarIntervalo(Persona.Actividades[i].Fin, Persona.Actividades[i + 1].Inicio, Duracion))
                {
                    HorariosDisponibles.Add(new BloqueTiempo
                    {
                        Inicio = Persona.Actividades[i].Fin,
                        Fin = Persona.Actividades[i + 1].Inicio
                    });
                }
            }

            if (VerificarIntervalo(Persona.Actividades.Last().Fin, Persona.JornadaLaboral.Fin, Duracion))
            {
                HorariosDisponibles.Add(new BloqueTiempo
                {
                    Inicio = Persona.Actividades.Last().Fin,
                    Fin = Persona.JornadaLaboral.Fin
                });
            }

            return HorariosDisponibles;
        }
    }
}
