using UnityEngine;
using VARLab.DeveloperTools;

namespace VARLab.MPCircuits
{
    public class LoadCircuitCommand : MonoBehaviour, ICommand
    {

        public string Name => "circuit";

        public string Usage => $"{Name}";

        public string Description => "";

        [Tooltip("Imports a preconfigured Circuit onto Circuitboard")]
        public CircuitImporter Importer;

        public bool Execute(CommandEventArgs e)
        {
            Debug.Log("Loading Preconfigured Circuit");
            Importer.StartCircuitImport();
            return true;
        }


        /// <summary>
        ///     On start, the load circuit command will add itself to the interpreter
        /// </summary>
        private void Start()
        {
            var cmd = CommandInterpreter.Instance;
            if (cmd != null && cmd.Add(this))
            {
                Debug.Log("Loaded the custom 'Circuit Builder' command");
            }
        }

    }
}
