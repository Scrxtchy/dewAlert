using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Configuration;

namespace dewAlert
{
    class Connection
    {
		private List<ServerConnection> Servers = new List<ServerConnection>();
		private List<string> Rules = new List<string>();
		Dictionary<string, int> Messages = new Dictionary<string, int>();

		public Connection()
		{
			Thread RconThread = new Thread(new ThreadStart(StartConnection));
			RconThread.Start();

			Thread KeepRconAlive = new Thread(new ThreadStart(KeepAlive));
			KeepRconAlive.Start();

			Thread ServerMessages = new Thread(new ThreadStart(ScheduledMessage));
			ServerMessages.Start();
		}

		public void StartConnection()
		{
			Console.WriteLine("Connecting");
			IConfiguration config = new ConfigurationBuilder().AddJsonFile("config.json", false).Build();

			foreach(IConfigurationSection server in config.GetSection("servers").GetChildren())
			{
				ServerConnection connection = new ServerConnection(server["address"], server["password"]);
				connection.ServerMessage += new ServerEventHandler(this.OnMessage);
				connection.ServerClose += new ServerCloseHandler(this.OnClose);
				Servers.Add(connection);
			}
			foreach(IConfigurationSection rule in config.GetSection("rules").GetChildren())
			{
				Rules.Add(rule.Value);
			}

			foreach (IConfigurationSection message in config.GetSection("messages").GetChildren())
			{
				Messages.Add(message["message"], int.Parse(message["wait"]));
			}
		}

		private void OnMessage(object sender, string message)
		{
			if (message.StartsWith("accept"))
			{
				Console.WriteLine("Connected");
			}	else if (RegexParser.IsChat(message))
			{
				ChatMessage cm = RegexParser.ParseChat(message);

				Console.WriteLine(cm);

				if (cm.message.ToLower() == "!rules")
				{
					ServerConnection sc = (ServerConnection)sender;

					foreach (string rule in Rules)
					{
						sc.Command(String.Format("Server.PM \"{0}\" \"{1}\"", cm.name, rule));
					}
				}
			}
#if DEBUG
			else
			{
				Console.WriteLine(message);
			}
#endif
		}

		private void OnClose(object sender, int code)
		{
			Console.WriteLine("Connection Closed");
		}

		public void KeepAlive()
		{
			Thread.Sleep(2000);
			while (true)
			{
				Thread.Sleep(120000);
			}
		}

		public void ScheduledMessage()
		{
			
			Thread.Sleep(2000);
			
			while (true)
			{
				foreach(KeyValuePair<string, int> message in Messages)
				{
					foreach(ServerConnection server in Servers)
					{
						server.Command("Server.Say " + message.Key);
					}
					Thread.Sleep(message.Value);
				}

			}

		}

	}
}
