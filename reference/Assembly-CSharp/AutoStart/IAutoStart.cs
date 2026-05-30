namespace AutoStart
{
	public interface IAutoStart
	{
		void EnableAutoStart();

		void DisableAutoStart();

		bool IsEnabled();
	}
}
