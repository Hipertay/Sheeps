using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Сorral : MonoBehaviour
{
    public int coralID;
    public List<GameObject> cattleCorral = new List<GameObject>();
    public List<MeshFilter> meshCattleCorral = new List<MeshFilter>();

    public Animator normalizer;
    public float timeCourotine;

    public IEnumerator StartNormalizer(bool b)
    {
        yield return new WaitForSeconds(timeCourotine);

       normalizer.SetBool("isNormalized", b);

    }
}


