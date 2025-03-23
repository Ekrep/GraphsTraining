using System;

namespace CollectionsTraining.CollectionUtils
{
    [System.Serializable]
    public class BoolWrapper
    {
        public bool value;
        public BoolWrapper(bool value)
        {
            this.value = value;

        }
        public BoolWrapper()
        {
            value = false;
        }

    }

    [System.Serializable]
    public class Command
    {
        public Delegate commandMethod;
        public object[] parameters;
        public Command(Delegate method, params object[] args)
        {
            commandMethod = method;
            parameters = args;
            CommandConsole.Instance.AssignCommandToBuffer(this);
        }
        public object Execute()
        {
            return commandMethod.DynamicInvoke(parameters);
        }
    }

}

