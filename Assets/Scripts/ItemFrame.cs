using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemFrame : MonoBehaviour {
    [SerializeField] public GameObject dropper;
    [SerializeField] public GameObject cover;
    [SerializeField] public GameObject spellIcon;
    private void Start() {
        dropper = transform.GetChild(0).gameObject;
        cover = transform.GetChild(1).gameObject;
        spellIcon = transform.GetChild(2).gameObject;
        dropper.SetActive(true);
        cover.SetActive(true);
        spellIcon.SetActive(false);
    }

    private void Update() {
        
    }

    public void PutOnCooldownFor(float cooldown) {
        dropper.SetActive(true);
        cover.SetActive(true);
        StartCoroutine(PutOnCooldownForCoro(cooldown));
    }

    private IEnumerator PutOnCooldownForCoro(float cooldown) {
        dropper.SetActive(true);
        cover.SetActive(true);
        for (int i = 0; i < cooldown*10; i++) {
            dropper.SetActive(true);
            cover.SetActive(true);
            dropper.transform.localPosition = new Vector2(0, -i*1.4f/(cooldown * 10));
            yield return new WaitForSeconds(0.1f);
        }
        dropper.transform.localPosition = new Vector2(0, 0);
        dropper.SetActive(false);
        cover.SetActive(false);
    }
    
    public void Unlock() {
        StartCoroutine(WTF());
    }
    
    private IEnumerator WTF() {
        yield return new WaitForSeconds(0.01f);
        dropper.SetActive(false);
        cover.SetActive(false);
        spellIcon.SetActive(true);
    }
}
