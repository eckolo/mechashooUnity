﻿using UnityEngine;
using System.Collections;
using System.Linq;

/// <summary>
/// 対古王国
/// </summary>
public class MainStage4_2 : MainStage4Origin
{
    public override bool challengeable
    {
        get {
            if(sys.storyPhase >= Configs.StoryPhase.GAME_CLEAR) return true;
            if(sys.dominance.republic < 0) return false;
            return base.challengeable;
        }
    }

    protected override IEnumerator OpeningAction()
    {
        yield return DisplayLocation(location);

        yield return sysPlayer.HeadingDestination(new Vector2(0, 0), sysPlayer.maximumSpeed);
        yield return sysPlayer.StoppingAction();
        yield return Wait(Configs.Window.DEFAULT_MOTION_TIME);

        yield return WaitMessages("人工頭脳", new[] {
            @"…所定の位置につきました。
ここで定刻まで待機です。",
            @"…そういえば、4国会談の件で遭遇した大型不明機についてなのですが。
あの機体、どうやら遺失時代の遺産ではないかとのことでした。",
            @"稀に古代遺跡から発掘される非常に特殊な機体ですね。
中でも戦闘用機体には高度な自動制御機構が組み込まれており、搭乗者無しでの長時間稼働も可能だとか。",
            @"そのためか過去に数例、遺跡発掘等の際に暴走を起こした事例が見られます。
いずれも再現不能な技術の用いられたかなり強力な機体であり、被害は甚大なものだったようです。",
            @"…我々は運がよかった、ということでしょうか。"
        });
        yield return Wait(Configs.Window.DEFAULT_MOTION_TIME);
        yield return WaitMessages("襲撃部隊司令所", new[] {
            @"本隊、突撃を開始。
後詰部隊は各自、後方より接近する防衛部隊を足止めせよ。"
        });
        yield return Wait(Configs.Window.DEFAULT_MOTION_TIME);
        yield return WaitMessages("人工頭脳", new[] {
            @"防衛部隊の足止め、了解。
戦闘行動を開始します。"
        });
        yield return WaitMessages("人工頭脳", new[] {
            @"さて、楽しい楽しい与太話はここまでです。
しっかり足止めの任を果たしましょう。"
        }, callSound: false);
        yield break;
    }

    protected override IEnumerator StageAction()
    {
        SetEnemy(0, new Vector2(-1.2f, 0.6f), 0, activityLimit: INTERVAL);
        SetEnemy(0, new Vector2(-1.2f, -0.6f), 0, activityLimit: INTERVAL);
        yield return Wait(INTERVAL_A_LITTLE);
        yield return Wait(INTERVAL_A_LITTLE);
        SetEnemy(2, new Vector2(-1.3f, -1.5f), 45, activityLimit: INTERVAL);
        SetEnemy(2, new Vector2(-1.3f, 1.5f), -45, activityLimit: INTERVAL);
        yield return Wait(INTERVAL_A_LITTLE);
        yield return Wait(INTERVAL_A_LITTLE);
        yield return Wait(INTERVAL_A_LITTLE);
        SetEnemy(1, new Vector2(-1.2f, 0.8f), -10, activityLimit: INTERVAL);
        SetEnemy(1, new Vector2(-1.2f, -0.8f), 10, activityLimit: INTERVAL);
        SetEnemy(2, new Vector2(-1.3f, 0), 0, activityLimit: INTERVAL, onTheWay: false);

        yield return WaitWave(INTERVAL);

        SetEnemy(2, new Vector2(-1.2f, -0.8f), 10, activityLimit: INTERVAL);
        SetEnemy(2, new Vector2(-1.2f, 0.8f), -10, activityLimit: INTERVAL);
        SetEnemy(0, new Vector2(-1.3f, -0.4f), 5, activityLimit: INTERVAL, onTheWay: false);
        SetEnemy(0, new Vector2(-1.3f, 0.4f), -5, activityLimit: INTERVAL, onTheWay: false);
        yield return Wait(INTERVAL_A_LITTLE);
        SetEnemy(3, new Vector2(-1.2f, 0), 0, activityLimit: INTERVAL + INTERVAL_A_LITTLE * 2, onTheWay: false);
        yield return Wait(INTERVAL_A_LITTLE);
        SetEnemy(0, new Vector2(-1.2f, -0.3f), 5, activityLimit: INTERVAL, onTheWay: false);
        SetEnemy(0, new Vector2(-1.2f, 0.3f), -5, activityLimit: INTERVAL, onTheWay: false);
        yield return Wait(INTERVAL_A_LITTLE);
        SetEnemy(1, new Vector2(-1.2f, -0.6f), 10, activityLimit: INTERVAL);
        SetEnemy(1, new Vector2(-1.2f, 0.6f), -10, activityLimit: INTERVAL);

        yield return WaitWave(INTERVAL);

        SetEnemy(1, new Vector2(-1.2f, 0.7f), -15, activityLimit: INTERVAL);
        SetEnemy(1, new Vector2(-1.3f, -0.1f), -15, activityLimit: INTERVAL);
        SetEnemy(3, new Vector2(-1.3f, 0.3f), -15, levelTweak: 5, onTheWay: false);
        yield return Wait(INTERVAL_A_LITTLE);
        yield return Wait(INTERVAL_A_LITTLE);
        yield return Wait(INTERVAL_A_LITTLE);
        SetEnemy(0, new Vector2(-1.2f, -0.7f), 15, activityLimit: INTERVAL);
        SetEnemy(0, new Vector2(-1.3f, 0.1f), 15, activityLimit: INTERVAL);
        SetEnemy(3, new Vector2(-1.3f, -0.3f), 15, levelTweak: 5, onTheWay: false);

        yield return Wait(() => !allEnemyObjects.Any());
        yield return WaitMessages("人工頭脳", new[] {
            @"本隊より連絡、主要設備の破壊に成功したとのことです。",
            @"予定通り、次の波を超えたところで撤退しますよ。"
        });

        SetEnemy(2, new Vector2(-1.2f, -0.8f), 20, activityLimit: INTERVAL);
        SetEnemy(2, new Vector2(-1.2f, 0.8f), -20, activityLimit: INTERVAL);
        yield return Wait(INTERVAL_A_LITTLE);
        SetEnemy(3, new Vector2(-1.2f, 0), 0, levelTweak: 5, activityLimit: INTERVAL + INTERVAL_A_LITTLE);
        yield return Wait(INTERVAL_A_LITTLE);
        SetEnemy(2, new Vector2(-1.2f, -0.4f), 10, activityLimit: INTERVAL);
        SetEnemy(2, new Vector2(-1.2f, 0.4f), -10, activityLimit: INTERVAL);

        yield return WaitWave(INTERVAL);

        SetEnemy(3, new Vector2(-1.2f, 0.8f), -30, activityLimit: INTERVAL + INTERVAL_A_LITTLE * 3);
        yield return Wait(INTERVAL_A_LITTLE);
        SetEnemy(0, new Vector2(-0.1f, 1.3f), -60, levelTweak: 5, activityLimit: INTERVAL);
        SetEnemy(0, new Vector2(-0.1f, -1.3f), 60, levelTweak: 5, activityLimit: INTERVAL);
        yield return Wait(INTERVAL_A_LITTLE);
        SetEnemy(0, new Vector2(-0.3f, 1.3f), -60, levelTweak: 5, activityLimit: INTERVAL);
        SetEnemy(0, new Vector2(-0.3f, -1.3f), 60, levelTweak: 5, activityLimit: INTERVAL);
        yield return Wait(INTERVAL_A_LITTLE);
        SetEnemy(0, new Vector2(-0.7f, 1.3f), -60, levelTweak: 5, activityLimit: INTERVAL);
        SetEnemy(0, new Vector2(-0.7f, -1.3f), 60, levelTweak: 5, activityLimit: INTERVAL);
        SetEnemy(3, new Vector2(-1.2f, -0.8f), 30, activityLimit: INTERVAL + INTERVAL_A_LITTLE * 2, onTheWay: false);
        yield return Wait(INTERVAL_A_LITTLE);
        SetEnemy(1, new Vector2(-0.9f, 1.3f), -60, levelTweak: 5, activityLimit: INTERVAL);
        SetEnemy(1, new Vector2(-0.9f, -1.3f), 60, levelTweak: 5, activityLimit: INTERVAL);
        yield return Wait(INTERVAL_A_LITTLE);
        SetEnemy(1, new Vector2(-0.5f, 1.3f), -90, levelTweak: 5, activityLimit: INTERVAL);
        SetEnemy(1, new Vector2(-0.5f, -1.3f), 90, levelTweak: 5, activityLimit: INTERVAL);

        yield return WaitWave(INTERVAL);

        SetEnemy(3, new Vector2(-1.2f, 0.5f), 0, levelTweak: 10, onTheWay: false);
        SetEnemy(3, new Vector2(-1.2f, -0.5f), 0, levelTweak: 10, onTheWay: false);
        yield return Wait(INTERVAL_A_LITTLE);
        SetEnemy(3, new Vector2(-1.2f, 0), 0, levelTweak: 15, onTheWay: false);

        yield return Wait(() => !allEnemyObjects.Any());
        yield return ProduceWarnings(600);
        yield return WaitMessages("人工頭脳", new[] {
            @"付近に転移反応を確認。",
            @"これは…かなりの大物です。
注意してください。"
        });

        SetEnemy(enemyList.Count - 1, new Vector2(-0.8f, -1.5f), 80, levelTweak: 12, onTheWay: false);
        yield break;
    }

    protected override IEnumerator SuccessAction()
    {
        yield return WaitMessages("人工頭脳", new[] {
            @"付近に敵影無し。
ひとしきり波は凌いだようです。",
            @"この機に脱出しましょう。"
        });

        var returningX = viewPosition.x - viewSize.x;
        var baseAim = sysPlayer.baseAimPosition;
        var armPosition = Vector2.left * Mathf.Abs(baseAim.x) + Vector2.up * baseAim.y;
        var returningPosition = new Vector2(returningX, sysPlayer.position.y + viewSize.y);
        yield return sysPlayer.HeadingDestination(returningPosition, sysPlayer.maximumSpeed, () => sysPlayer.Aiming(armPosition + sysPlayer.position, siteSpeedTweak: 2));
        yield return sysPlayer.StoppingAction();

        yield return WaitMessages("人工頭脳", new[] {
            @"お疲れ様でした。"
        }, callSound: false);

        if(!isCleared && sys.storyPhase < Configs.StoryPhase.END_FOUR_PRACTICE)
        {
            sys.dominance.republic++;
            sys.dominance.oldKingdom--;
        }
        yield return base.SuccessAction();
        yield break;
    }
}
