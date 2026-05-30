using System.Diagnostics;
using UnityEngine;

namespace BongoCat
{
	public class OpenLogFolder : MonoBehaviour
	{
		private string _command;

		private string _logPath;

		private void Awake()
		{
			_command = "explorer.exe";
			_logPath = "/select, \"" + Application.persistentDataPath.Replace('/', '\\') + "\\Player.log\"";
		}

		public void OpenInExplorer()
		{
			Process.Start(_command, _logPath);
		}
	}
}
