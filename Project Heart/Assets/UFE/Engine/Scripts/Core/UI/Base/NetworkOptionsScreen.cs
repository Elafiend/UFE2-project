using System.Net;

namespace UFE3D
{
	public class NetworkOptionsScreen : UFEScreen
	{

		public virtual void GoToRandomMatchScreen()
		{
			UFE.StartSearchMatchScreen();
		}

		public virtual void GoToRoomMatchScreen()
		{
			UFE.StartRoomMatchScreen();
		}

		public virtual void GoToDirectMatchScreen()
		{
			// TODO: Add direct IP connection
		}

		public virtual void GoToBluetoothScreen()
		{
			UFE.StartBluetoothGameScreen();
		}

		public virtual void GoToMainMenu()
		{
			UFE.StartMainMenuScreen();
		}

		public virtual string GetIp()
		{
			string hostName = System.Net.Dns.GetHostName();
			IPHostEntry ipHostEntry = System.Net.Dns.GetHostEntry(hostName);
			IPAddress[] ipAddresses = ipHostEntry.AddressList;

			return ipAddresses[ipAddresses.Length - 1].ToString();
		}
	}
}