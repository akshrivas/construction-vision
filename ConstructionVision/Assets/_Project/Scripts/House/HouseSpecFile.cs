using System;

[Serializable]
public class HouseSpecFile
{
    public string id;
    public PlotSize plot;
    public int floors;
    public int bedrooms;
    public int parking;
    public int familySize;
    public string orientation;
    public string[] rooms;
    public bool balcony;
    public string createdAt;
    public string status;

    public bool IsSpikeHouse()
    {
        if (plot == null || floors != 2 || bedrooms != 3 || parking != 1 || !balcony)
            return false;
        if (Math.Abs(plot.width - 30f) > 0.01f || Math.Abs(plot.length - 50f) > 0.01f)
            return false;

        var bedroomCount = 0;
        var hasLiving = false;
        var hasKitchen = false;
        if (rooms == null)
            return false;
        foreach (var room in rooms)
        {
            if (room == "living") hasLiving = true;
            else if (room == "kitchen") hasKitchen = true;
            else if (room == "bedroom") bedroomCount += 1;
        }
        return hasLiving && hasKitchen && bedroomCount >= 3;
    }
}

[Serializable]
public class PlotSize
{
    public float width;
    public float length;
}
