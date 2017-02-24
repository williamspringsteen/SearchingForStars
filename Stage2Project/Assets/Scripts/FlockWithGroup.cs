using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FlockWithGroup : MonoBehaviour
{
    [SerializeField]
    private GroupTag.Group GroupCode;

    [SerializeField]
    private float Speed;

    [SerializeField]
    private float BuddyDistance = 100.0f;

    [SerializeField]
    private float AvoidDistance = 1.0f;

    [SerializeField]
    private float CheckForBuddiesInterval = 10.0f;

    private List<GroupTag> mCurrentBuddies;
    private Rigidbody mBody;
    private float mCountDownToCheck;

    void Awake()
    {
        mCurrentBuddies = new List<GroupTag>();
        mBody = GetComponent<Rigidbody>();
        mCountDownToCheck = 0.0f;
    }

    void Update()
    {
        mCountDownToCheck -= Time.deltaTime;
        if (mCountDownToCheck <= 0.0f)
        {
            UpdateBuddyList();
            mCountDownToCheck = CheckForBuddiesInterval;
        }

        FlockWithBuddies();
    }

    //Buddy list is every object of the specified group type within BuddyDistance
    internal void UpdateBuddyList()
    {
        GroupTag[] individuals = FindObjectsOfType<GroupTag>();

        for (int count = 0; count < individuals.Length; ++count)
        {
            if (individuals[count].gameObject != gameObject && individuals[count].Affiliation == GroupCode )
            {
                Vector3 difference = individuals[count].transform.position - transform.position;
                if (difference.magnitude <= BuddyDistance)
                {
                    if (!mCurrentBuddies.Contains(individuals[count]))
                    {
                        mCurrentBuddies.Add(individuals[count]);
                    }
                }
                else if (mCurrentBuddies.Contains(individuals[count]))
                {
                    mCurrentBuddies.Remove(individuals[count]);
                }
            }
        }
    }

    private void FlockWithBuddies()
    {
        if (mCurrentBuddies.Count > 0)
        {
            //align will be the normalised average of your buddies' velocities
            //(So it is the average direction your buddies are moving in)
            Vector3 align = Vector3.zero;
            //cohesion is the average coordinates of your buddies, minus your coordinates, then normalised 
            //(So it is the direction of your buddies' centre of mass from your current position)
            Vector3 cohesion = Vector3.zero; 
            //avoid is your coordinates, minus the average coordinates of your CLOSE buddies, then normalised
            //(So it is the opposite of the direction of your CLOSE buddies' centre of mass from your current position)
            Vector3 avoid = Vector3.zero;

            //Required so we can just calculate the average of close buddies
            int mAvoidBuddiesCount = 0;
            
            for (int count = 0; count < mCurrentBuddies.Count; ++count)
            {
                if (mCurrentBuddies[count] != null)
                {
                    Rigidbody body = mCurrentBuddies[count].GetComponent<Rigidbody>();
                    align += body.velocity;
                    cohesion += mCurrentBuddies[count].transform.position;
                    if ((mCurrentBuddies[count].transform.position - transform.position).magnitude < AvoidDistance)
                    {
                        avoid += mCurrentBuddies[count].transform.position;
                        mAvoidBuddiesCount++;
                    }
                }
            }

            align /= mCurrentBuddies.Count;
            cohesion /= mCurrentBuddies.Count;
            avoid /= mAvoidBuddiesCount;
            //avoid /= mCurrentBuddies.Count;

            align.Normalize();
            cohesion = cohesion - transform.position;
            cohesion.Normalize();
            avoid = transform.position - avoid;
            avoid.Normalize();

            //Combine the directions found (all of the same order of magnitude: normalised), and then apply force in that direction.
            //Basically, we want to flock with everyone close enough, specified by buddyDistance, but want space in between, as specified by avoidDistance.
            mBody.AddForce(( align + cohesion + avoid) * Speed * Time.deltaTime);
        }
    }
}
