using UnityEngine;

public class BoxManager : MonoBehaviour
{
    public ObservableList<Box> boxes = new ObservableList<Box>();

    void OnCollisionEnter(Collision collision)
    {
        Box box = collision.transform.GetComponent<Box>();
        if (box)
        {
            if (!boxes.Contains(box))
            {
                boxes.Add(box);
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        Box box = collision.transform.GetComponent<Box>();
        if (box)
        {
            boxes.Remove(box);
        }
    }
}