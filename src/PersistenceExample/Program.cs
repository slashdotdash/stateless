using System;
using Stateless;

namespace PersistenceExample
{
    static class Program
    {
        static void Main()
        {
            try
            {
                const string on = "On";
                const string off = "Off";
                const char space = ' ';

                Console.WriteLine("Press <space> to toggle the switch. Any other key will raise an error.");

                var savedState = off;
                Func<string> loadMethod = () =>
                {
                    Console.WriteLine("(Persistence: state {0} has been loaded)", savedState);
                    return savedState;
                };
                Action<string> saveMethod = state =>
                {
                    savedState = state; 
                    Console.WriteLine("(Persistence: state {0} has been saved)", state);
                };

                var initialState = loadMethod();
                var onOffSwitch = new StateMachine<string, char>(initialState, saveMethod);

                onOffSwitch.Configure(off).Permit(space, on);
                onOffSwitch.Configure(on).Permit(space, off);

                while (true)
                {
                    Console.WriteLine("Switch is in state: " + onOffSwitch.State);
                    var pressed = Console.ReadKey(true).KeyChar;
                    onOffSwitch.Fire(pressed);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey(true);
            }
        }
    }
}
