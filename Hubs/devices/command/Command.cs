using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eDocumentReader.Hubs.devices.command
{
    /// <summary>
    /// The system use Command to control the io devices.
    /// </summary>
    public class Command
    {
        private List<Object> payload = new List<Object>();
        private CommandType commandType;
        public Command(CommandType type)
        {
            commandType = type;
        }
        public Command(CommandType type, List<Object> pl){
            commandType = type;
            payload = pl;
        }

        public void addData(Object data)
        {
            payload.Add(data);
        }

        public List<Object> getPayload() { return payload; }
        public CommandType getType() { return commandType; }

    }
}