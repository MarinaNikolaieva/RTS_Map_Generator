namespace VoronoiMapTrial.Physical_Map
{
    public class Resource
    {
        public ResourceType type;
        public string name;
        public int ID;
        public string code;  //unicode symbol

        public int strengh;

        public Resource(ResourceType type, string name, int id, string code, int strength)
        {
            this.type = type;
            this.name = name;
            this.ID = id;
            this.code = code;
            this.strengh = strength;
        }
    }
}
