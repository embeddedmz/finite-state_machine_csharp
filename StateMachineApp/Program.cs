using System;
using System.Threading;
using FiniteStateMachine;

namespace StateMachineApp
{
    class Program
    {
        private enum TrafficLightsState
        {
            Green,
            Yellow,
            Red
        }

        static void Main(string[] args)
        {
            var trafficLights = new FiniteStateMachine<TrafficLightsState>();

            trafficLights.RegisterState(TrafficLightsState.Green,
                () => { Console.WriteLine("Green ON"); Thread.Sleep(3000); },
                () => { Console.WriteLine("Green OFF"); });

            trafficLights.RegisterState(TrafficLightsState.Yellow,
                () => { Console.WriteLine("Yellow ON"); Thread.Sleep(1000); },
                () => { Console.WriteLine("Yellow OFF"); });

            trafficLights.RegisterState(TrafficLightsState.Red,
                () => { Console.WriteLine("Red ON"); Thread.Sleep(2000); },
                () => { Console.WriteLine("Red OFF"); });

            bool green = false;
            bool yellow = false;
            bool red = false;

            trafficLights.RegisterTransition(TrafficLightsState.Green, TrafficLightsState.Yellow, () => yellow);
            trafficLights.RegisterTransition(TrafficLightsState.Yellow, TrafficLightsState.Red, () => red);
            trafficLights.RegisterTransition(TrafficLightsState.Red, TrafficLightsState.Green, () => green);

            trafficLights.Start(TrafficLightsState.Green);

            yellow = true;
            trafficLights.Update();
            yellow = false;

            red = true;
            trafficLights.Update();
            red = false;

            green = true;
            trafficLights.Update();
        }
    }
}
