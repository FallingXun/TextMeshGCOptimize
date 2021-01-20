using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GenerateText : MonoBehaviour
{
    public Transform m_Root;
    public TextMeshProUGUI m_Default;
    public int m_Count = 20;
    public bool m_HasChild = false;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CreateText();
        }
    }

    private void CreateText()
    {
        if (m_Root != null)
        {
            if (m_HasChild == false)
            {
                for (int i = 0; i < m_Count; i++)
                {
                    var item = GameObject.Instantiate(m_Default, m_Root);
                    item.gameObject.SetActive(true);
                    //item.transform.localPosition = new Vector3(Random.Range(-568, 568), Random.Range(-360, 360));
                }
                m_HasChild = true;
            }
            else
            {
                for (int i = m_Root.childCount - 1; i >= 0; i--)
                {
                    Destroy(m_Root.GetChild(i).gameObject);
                }
                m_HasChild = false;
            }
        }
        
    }
}
