namespace UserStateManager
{
    public enum UserStateType
    {
        None
    }

    public class UserState
    {
        public UserStateType State { get; set; } = UserStateType.None;
    }
}
