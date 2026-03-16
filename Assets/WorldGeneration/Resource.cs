[System.Serializable]
public class Resource
{
    public ResourceType resourceType;
    public enum ResourceType
    {
        Wheat = 0,
        Fish = 1,
        Cows = 2,
        Horses = 3,
        Stone = 4,
        Iron = 5,
    }
    
    public Resource(ResourceType resource)
    {
        resourceType = resource;
    }
}
