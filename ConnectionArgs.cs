namespace PortPing
{
    public class ConnectionArgs
    {
        public string Hostname { get; set; }
        public int Port { get; set; }

        public ConnectionArgs(string hostname, int port)
        {
            this.Hostname = hostname;
            this.Port = port;
        }
    }
}
