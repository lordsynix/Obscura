using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CastleUI : MonoBehaviour
{
    private Button button;
    [SerializeField] private Button visitButton;
    [SerializeField] private Button attackButton;
    [SerializeField] private Button defendButton;
    [SerializeField] private Button sendButton;

    private float lastPacketSend;
    private float packetSendCooldown = 0.2f;

    private void Start()
    {
        lastPacketSend = Time.time;
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        if (GameUI.Instance.originSelect)
        {
            GameUI.Instance.SelectOrigin(gameObject);
        }
        if (GameUI.Instance.targetSelect)
        {
            GameUI.Instance.SelectTarget(gameObject);
        }
        if (GameUI.Instance.originSelect == false && GameUI.Instance.targetSelect == false)
        {
            GameUI.Instance.DisplayCastleOptions(gameObject);
        }
    }

    public void Visit()
    {

    }

    public void Attack()
    {
        if (lastPacketSend + packetSendCooldown < Time.time)
        {
            GameManager.Instance.AttackServerRpc(int.Parse(gameObject.name), NetworkManager.Singleton.LocalClientId);
            lastPacketSend = Time.time;
        }      
    }

    public void Defend()
    {

    }

    public void Send()
    {

    }
}
