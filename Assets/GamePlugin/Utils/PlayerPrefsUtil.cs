using System;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

namespace mygame.sdk
{
    public class PlayerPrefsUtil
    {
        public static int getCurrItemFashion(string keyItem)
        {
            return PlayerPrefsBase.Instance().getInt("itemgamefashion_" + keyItem, -1);
        }

        public static void setCurrItemFashion(string keyItem, int idx)
        {
            PlayerPrefsBase.Instance().setInt("itemgamefashion_" + keyItem, idx);
        }
        public static int cfBossHealth
        {
            get { return PlayerPrefsBase.Instance().getInt("cf_boss_health", 500); }
            set { PlayerPrefsBase.Instance().setInt("cf_boss_health", value); }
        }
         public static int cfBossDamage
        {
            get { return PlayerPrefsBase.Instance().getInt("cf_boss_damage", 10); }
            set { PlayerPrefsBase.Instance().setInt("cf_boss_damage", value); }
        }
        public static string Yesterday
        {
            get { return PlayerPrefsBase.Instance().getString("sf_yesterday", ""); }
            set { PlayerPrefsBase.Instance().setString("sf_yesterday", value); }
        }
        public static int getSubLevelMiniGame(string keyGame)
        {
            return PlayerPrefsBase.Instance().getInt("levelgame_" + keyGame, 1);
        }

        public static void setSubLevelMiniGame(string keyGame, int level)
        {
            PlayerPrefsBase.Instance().setInt("levelgame_" + keyGame, level);
        }
        public static int enableZoom
        {
            get { return PlayerPrefsBase.Instance().getInt("apply_zoom_setting", 1); }
            set { PlayerPrefsBase.Instance().setInt("apply_zoom_setting", value); }
        }
        public static int levelShowRating
        {
            get { return PlayerPrefsBase.Instance().getInt("level_show_rate", 1); }
            set { PlayerPrefsBase.Instance().setInt("level_show_rate", value); }
        }

        public static int isShowRate
        {
            get { return PlayerPrefsBase.Instance().getInt("is_show_rate", 0); }
            set { PlayerPrefsBase.Instance().setInt("is_show_rate", value); }
        }

        public static int SoundFxSetting
        {
            get { return PlayerPrefsBase.Instance().getInt("SoundFxSetting", 1); }
            set { PlayerPrefsBase.Instance().setInt("SoundFxSetting", value); }
        }

        public static int MusicSetting
        {
            get { return PlayerPrefsBase.Instance().getInt("MusicSetting", 1); }
            set { PlayerPrefsBase.Instance().setInt("MusicSetting", value); }
        }
        public static int BgMusicSetting
        {
            get { return PlayerPrefsBase.Instance().getInt("key_config_bgmusic", 0); }
            set { PlayerPrefsBase.Instance().setInt("key_config_bgmusic", value); }
        }

        public static int VibrateSetting
        {
            get { return PlayerPrefsBase.Instance().getInt("key_config_vibrate", 1); }
            set { PlayerPrefsBase.Instance().setInt("key_config_vibrate", value); }
        }

        public static int CountSpin5
        {
            get { return PlayerPrefsBase.Instance().getInt("count_spin_5", 0); }
            set { PlayerPrefsBase.Instance().setInt("count_spin_5", value); }
        }
        public static int CountJackPot
        {
            get { return PlayerPrefsBase.Instance().getInt("count_jackpot", 0); }
            set { PlayerPrefsBase.Instance().setInt("count_jackpot", value); }
        }
        public static int CountTutBuilding
        {
            get { return PlayerPrefsBase.Instance().getInt("count_tutbuilding", 1); }
            set { PlayerPrefsBase.Instance().setInt("count_tutbuilding", value); }
        }
        public static int FirstTutChooseDecent
        {
            get { return PlayerPrefsBase.Instance().getInt("first_choose_decent", 0); }
            set { PlayerPrefsBase.Instance().setInt("first_choose_decent", value); }
        }

        public static int memBonus
        {
            get { return PlayerPrefsBase.Instance().getInt("play_level_bonus", -2); }
            set { PlayerPrefsBase.Instance().setInt("play_level_bonus", value); }
        }
        public static string MemMapLevel
        {
            get { return PlayerPrefsBase.Instance().getString("mem_map", ""); }
            set { PlayerPrefsBase.Instance().setString("mem_map", value); }
        }
        public static int isSecondPlay
        {
            get { return PlayerPrefsBase.Instance().getInt("second_play", 0); }
            set { PlayerPrefsBase.Instance().setInt("second_play", value); }
        }

        public static int CurrentKey
        {
            get { return PlayerPrefsBase.Instance().getInt("current_key", 0); }
            set { PlayerPrefsBase.Instance().setInt("current_key", value); }
        }
        public static int LevelAttack
        {
            get { return PlayerPrefsBase.Instance().getInt("level_attack", 1); }
            set { PlayerPrefsBase.Instance().setInt("level_attack", value); }
        }

        public static int LevelHealth
        {
            get { return PlayerPrefsBase.Instance().getInt("level_health", 1); }
            set { PlayerPrefsBase.Instance().setInt("level_health", value); }
        }
        public static int LevelGoldBonus
        {
            get { return PlayerPrefsBase.Instance().getInt("level_gold", 1); }
            set { PlayerPrefsBase.Instance().setInt("level_gold", value); }
        }

        public static int CurrentEnemyEquip
        {
            get { return PlayerPrefsBase.Instance().getInt("enemy_equip", 1); }
            set { PlayerPrefsBase.Instance().setInt("enemy_equip", value); }
        }
        public static string SkinOwned
        {
            get { return PlayerPrefsBase.Instance().getString("skin_owned", ""); }
            set { PlayerPrefsBase.Instance().setString("skin_owned", value); }
        }
        public static int PerGetOnlyKeyOrGift
        {
            get { return PlayerPrefsBase.Instance().getInt("PerGetOnlyKeyOrGift", 10); }
            set { PlayerPrefsBase.Instance().setInt("PerGetOnlyKeyOrGift", value); }
        }
        public static int countKeyGift
        {
            get { return PlayerPrefsBase.Instance().getInt("countKeyGift", 0); }
            set { PlayerPrefsBase.Instance().setInt("countKeyGift", value); }
        }

        public static int PerGetKeyAndGift
        {
            get { return PlayerPrefsBase.Instance().getInt("PerGetKeyAndGift", 30); }
            set { PlayerPrefsBase.Instance().setInt("PerGetKeyAndGift", value); }
        }

        public static string CurrentBonusGift
        {
            get { return PlayerPrefsBase.Instance().getString("CurrentBonusGift", "head1"); }
            set { PlayerPrefsBase.Instance().setString("CurrentBonusGift", value); }
        }
        public static int ProcessGiftBonus
        {
            get { return PlayerPrefsBase.Instance().getInt("ProcessGiftBonus", 0); }
            set { PlayerPrefsBase.Instance().setInt("ProcessGiftBonus", value); }
        }

        public static int AudioSetting
        {
            get { return PlayerPrefsBase.Instance().getInt("audio_setting", 1); }
            set { PlayerPrefsBase.Instance().setInt("audio_setting", value); }
        }



        public static int deltaShowAdDeath
        {
            get { return PlayerPrefsBase.Instance().getInt("dt_show_ad_death", 1); }
            set { PlayerPrefsBase.Instance().setInt("dt_show_ad_death", value); }
        }
        public static int BossLevel
        {
            get { return PlayerPrefsBase.Instance().getInt("boss_level", 2); }
            set { PlayerPrefsBase.Instance().setInt("boss_level", value); }
        }
        public static int CharacterIndex
        {
            get { return PlayerPrefsBase.Instance().getInt("character_index", 0); }
            set { PlayerPrefsBase.Instance().setInt("character_index", value); }
        }
        public static int LevelEnemy
        {
            get { return PlayerPrefsBase.Instance().getInt("level_enemy", 0); }
            set { PlayerPrefsBase.Instance().setInt("level_enemy", value); }
        }
        public static int numEnemy
        {
            get { return PlayerPrefsBase.Instance().getInt("num_enemy", 3); }
            set { PlayerPrefsBase.Instance().setInt("num_enemy", value); }
        }
        public static int levelIndex
        {
            get { return PlayerPrefsBase.Instance().getInt("level_index", -1); }
            set { PlayerPrefsBase.Instance().setInt("level_index", value); }
        }
    }
}