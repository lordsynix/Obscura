using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CastleUI : MonoBehaviour
{
    public GameObject castleInformation;

    private float lastPacketSend;
    private float packetSendCooldown = 0.2f;

    private void Start()
    {
        lastPacketSend = Time.time;
    }

    public void OnButtonClicked()
    {
        if (castleInformation == null)
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

    public void SetCastleInformation(GameObject castleInformationPrefab)
    {
        castleInformation = castleInformationPrefab;
        Button[] buttons = castleInformation.GetComponentsInChildren<Button>();
        buttons[0].onClick.AddListener(Visit);
        buttons[1].onClick.AddListener(Attack);
        buttons[2].onClick.AddListener(Defend);
        buttons[3].onClick.AddListener(Send);
    }
}
