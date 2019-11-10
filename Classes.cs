using System;

public class Class1
{
    public class RoomUser
    {
        public DateTime startTime;
        public DateTime endTime;
        public string userName;

        public RoomUser(string userName, DateTime startTime, DateTime endTime)
        {
            this.userName = userName;
            this.startTime = startTime;
            this.endTime = endTime;
        }

        public static bool isValidUserData(string userName, DateTime startTime, DateTime endTime)
        {
            Regex r = new Regex("^[a-zA-Z0-9]*$");
            if (r.IsMatch(userName))
            {
                return false;
            }
            if (DateTime.Compare(startTime, endTime) >= 0)
            {
                return false;
            }
            return true;
        }
    }

    public class Room
    {
        public string roomName;
        public List<RoomUser> usersList;

        public Room(string roomName)
        {
            this.roomName = roomName;
            usersList = new List<RoomUser>();
        }

        public Boolean addUser(RoomUser newUser)
        {
            foreach (RoomUser user in usersList)
            {
                if (DateTime.Compare(newUser.startTime, user.endTime) < 0 && DateTime.Compare(newUser.startTime, user.startTime) >= 0 ||
                    DateTime.Compare(newUser.endTime, user.endTime) <= 0 && DateTime.Compare(newUser.endTime, user.startTime) > 0)
                {
                    return false;
                }
            }
            usersList.Add(newUser);
            return true;
        }

        public void cleanAll()
        {
            usersList = new List<RoomUser>();
        }
    }
}
