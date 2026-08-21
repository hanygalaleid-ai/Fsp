#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Fsp.EditorTools
{
    public sealed class BmgAuthored3DAssetBuildGuard : IPreprocessBuildWithReport
    {
        private static readonly string[] RequiredAssets =
        {
            "Assets/Fsp/Art/Resources/Models/BMG/bmg_assault_rifle_mk1.obj",
            "Assets/Fsp/Art/Resources/Models/BMG/bmg_smg_mk1.obj",
            "Assets/Fsp/Art/Resources/Models/BMG/bmg_sniper_mk1.obj",
            "Assets/Fsp/Art/Resources/Models/BMG/bmg_shotgun_mk1.obj",
            "Assets/Fsp/Art/Resources/Models/BMG/bmg_weapon_optic_mk1.obj",
            "Assets/Fsp/Art/Resources/Models/BMG/bmg_helmet_mk1.obj",
            "Assets/Fsp/Art/Resources/Models/BMG/bmg_face_mask_mk1.obj",
            "Assets/Fsp/Art/Resources/Models/BMG/bmg_backpack_mk1.obj",
            "Assets/Fsp/Art/Resources/Models/BMG/bmg_tactical_vest_mk1.obj",
            "Assets/Fsp/Art/Resources/Models/BMG/bmg_combat_boot_mk1.obj",
            "Assets/Fsp/Art/Resources/Models/BMG/bmg_tactical_glove_mk1.obj",
            "Assets/Fsp/Art/Resources/Models/BMG/bmg_knee_pad_mk1.obj",
            "Assets/Fsp/Art/Resources/Models/BMG/bmg_loot_crate_mk1.obj",
            "Assets/Fsp/Art/Resources/Models/BMG/bmg_buggy_mk1.obj",
            "Assets/Fsp/Art/Resources/Models/BMG/bmg_desert_car_mk1.obj",
            "Assets/Fsp/Art/Resources/Models/BMG/bmg_vehicle_wheel_mk1.obj",
            "Assets/Fsp/Art/Resources/Models/BMG/bmg_vehicle_bumper_mk1.obj",
            "Assets/Fsp/Art/Resources/Models/BMG/bmg_vehicle_seat_mk1.obj",
            "Assets/Fsp/Art/Resources/Models/BMG/bmg_vehicle_windshield_mk1.obj",
            "Assets/Fsp/Art/Resources/Models/BMG/bmg_vehicle_light_mk1.obj",
            "Assets/Fsp/Art/Resources/Models/BMG/bmg_watchtower_mk1.obj",
            "Assets/Fsp/Art/Resources/Models/BMG/bmg_guardhouse_mk1.obj",
            "Assets/Fsp/Art/Resources/Models/BMG/bmg_generator_mk1.obj",
            "Assets/Fsp/Art/Resources/Models/BMG/bmg_fuel_tank_mk1.obj",
            "Assets/Fsp/Art/Resources/Models/BMG/bmg_radio_mast_mk1.obj",
            "Assets/Fsp/Art/Resources/Models/BMG/bmg_supply_crate_mk1.obj",
            "Assets/Fsp/Art/Resources/Models/BMG/bmg_sandbag_wall_mk1.obj",
            "Assets/Fsp/Art/Resources/Models/BMG/bmg_shipping_container_mk1.obj",
            "Assets/Fsp/Art/Resources/Models/BMG/bmg_oil_barrel_mk1.obj",
            "Assets/Fsp/Art/Resources/Models/BMG/bmg_fence_panel_mk1.obj",
            "Assets/Fsp/Art/Resources/Models/BMG/bmg_transport_plane_mk1.obj",
            "Assets/Fsp/Art/Resources/Models/BMG/bmg_parachute_mk1.obj",
            "Assets/Fsp/Art/Resources/Models/BMG/bmg_male_torso_mk1.obj",
            "Assets/Fsp/Art/Resources/Models/BMG/bmg_female_torso_mk1.obj",
            "Assets/Fsp/Art/Resources/Models/BMG/bmg_head_mk1.obj",
            "Assets/Fsp/Art/Resources/Models/BMG/bmg_arm_mk1.obj",
            "Assets/Fsp/Art/Resources/Models/BMG/bmg_leg_mk1.obj",
            "Assets/Fsp/Art/Resources/Models/BMG/bmg_hangar_mk1.obj",
            "Assets/Fsp/Art/Resources/Models/BMG/bmg_barricade_mk1.obj",
            "Assets/Fsp/Art/Resources/Models/BMG/bmg_rock_cluster_mk1.obj",
            "Assets/Fsp/Art/Resources/Models/BMG/bmg_warehouse_mk1.obj",
            "Assets/Fsp/Art/Resources/Models/BMG/bmg_quarry_crusher_mk1.obj",
            "Assets/Fsp/Art/Resources/Models/BMG/bmg_aircraft_wreck_mk1.obj",
            "Assets/Fsp/Art/Resources/Models/BMG/bmg_port_crane_mk1.obj",
            "Assets/Fsp/Art/Resources/Models/BMG/bmg_water_tower_mk1.obj",
            "Assets/Fsp/Art/Resources/Models/BMG/bmg_lighthouse_mk1.obj",
            "Assets/Fsp/Art/Resources/Models/BMG/bmg_coast_hut_mk1.obj",
            "Assets/Fsp/Art/Resources/Models/BMG/bmg_barn_mk1.obj",
            "Assets/Fsp/Art/Resources/Models/BMG/bmg_pump_house_mk1.obj",
            "Assets/Fsp/Art/Resources/Models/BMG/bmg_old_crown_house_mk1.obj",
            "Assets/Fsp/Art/Resources/Models/BMG/bmg_crown_monument_mk1.obj"
        };
        public int callbackOrder => -850;
        public void OnPreprocessBuild(BuildReport report)
        {
            for(int i=0;i<RequiredAssets.Length;i++) if(AssetDatabase.LoadMainAssetAtPath(RequiredAssets[i])==null) throw new BuildFailedException("BMG authored 3D asset is missing: "+RequiredAssets[i]);
            Debug.Log($"BMG AUTHORED 3D GATE PASSED ({RequiredAssets.Length} required meshes).");
        }
    }
}
#endif
