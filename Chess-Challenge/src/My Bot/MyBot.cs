using ChessChallenge.API;
using System;
using System.Collections;
using System.Numerics;

public class MyBot : IChessBot
{
    int[,,] pst = new int[2, 6, 64];

    int[] value_mg = { 82, 237, 365, 477, 1025, 0 };
    int[] value_eg = { 94, 281, 297, 512, 936, 0 };

    // value of pieces by game phase from PeSTO
    int[] value_gp = { 0, 1, 1, 2, 4, 0 };

    Move best_move;

    const int max_depth = 3;

    const int table_size = 1 << 20;
    (ulong, int, int, int, Move)[] tt = new (ulong, int, int, int, Move)[table_size];

    public MyBot()
    {
        // raw data in constructor saves 99,840 bytes after gc lmao
        // 1st row is mg, 2nd row is eg
        decimal[] raw_mgeg_table = {324454911572041844052197376m, 75076251637434867814284861m, 29123027899232255186595905m, 679463589935167471377853969m, 622764552386721285493885210m, 9783870073220884m, 853792511368197730866298880m, 660107126339636692766873359m, 42355312132823125893347631m, 31507670913842921742821907m, 41193295289308132637156892m, 666184410397949141357830169m, 11019859289303915835304809m, 651696172032278943469292370m, 121058234499184519289533982m, 623839121419587375156298789m, 36267811662495560175459078m, 75696286717145482010127m, 649257115644407590980692487m, 104114097994879113462164263m, 47172125096491727721087003m, 62899666870273375367088154m, 635939857660055141050500632m, 699991661452951898981543430m, 651703762498292834060345644m, 29151173424785625276490241m, 139102041054936954514987067m, 138040011692296212609002524m, 683085603252313994626742043m, 625066618091819514183947529m, 36305572174512961990690053m, 741220517434551086583382787m, 621524939860012836400934721m, 657665316748116284395360020m, 680769522489397229590678804m, 743647989619289918984833294m, 16932298947359490929466638m, 749591013482441976939708680m, 1885485064m,
                                    419128914820612491296374784m, 162398385016552208672296094m, 10889823461391402009127480m, 621404478098643831804404241m, 19406643487212640098848260m, 9253627301729288m, 685609957376361150176821248m, 746023164048269947162773311m, 8545505272307244887320856m, 60524270313517289798380566m, 626270941214619540590174736m, 726675582325096560821153044m, 671022900017996762837837597m, 648019698721222534142759179m, 15727850803858238216477443m, 4849962864146558573348864m, 627491617637458285197264646m, 622632385744292846723404808m, 658909485323780599656092164m, 12127148025175477157503749m, 16958045854339547543116299m, 2479270154810991257324039m, 634709704167874128796451330m, 658904827232388861928806152m, 6089524068962989632523526m, 65386094019524721194172931m, 60720566972174612052846107m, 21850713382660521851879454m, 68994107384350856324328451m, 14580560447633344444522003m, 675867604947451165696074249m, 719407971043805646467514660m, 42371590137476262778259274m, 36376545995402102452789774m, 79912135087302895496878612m, 645609101597349316523460122m, 646905261709119178293511955m, 646877163543929968236435972m, 40204639516m };

        // pre process the pst to reduce method calls to query the raw table

        foreach (int phase in new[] {0, 1}) {
            // only go to 384, because last 6 are empty 0s off the board to make multiples of 10
            for (int i = 0; i < 384; i++) {
                BigInteger segment = (BigInteger.Parse(raw_mgeg_table[i / 10].ToString()) >> 9 * (i % 10)) & 0x1FF;

                pst[phase, i / 64, i % 64] = (short)((segment & 0xFF) * ((segment & (1 << 8)) != 0 ? -1 : 1));
            }
        }
    }
    int Eval(Board board)
    {

        int gamePhase = 0, mg_sum = 0, eg_sum = 0;

        PieceList[] all_pl = board.GetAllPieceLists();

        foreach (PieceList pl in all_pl)
        {
            int p_type_ind = (int)pl.TypeOfPieceInList - 1;

            for (int i = 0; i < pl.Count; i++)
            {
                Piece p = pl.GetPiece(i);

                gamePhase += value_gp[p_type_ind];

                // if white flip board, otherwise dont (because arrays listed in POV of white)
                int tb_ind(int n) => (7 - n / 8) * 8 + (n % 8);
                int ind = p.IsWhite ? tb_ind(p.Square.Index) : p.Square.Index;

                int neg = board.IsWhiteToMove == p.IsWhite ? 1 : -1;

                mg_sum += (pst[0, p_type_ind, ind] + value_mg[p_type_ind]) * neg;
                eg_sum += (pst[1, p_type_ind, ind] + value_eg[p_type_ind]) * neg;
            }
        }

        // in case of promotion resulting in > 24
        gamePhase = Math.Min(gamePhase, 24);

        int sum = (mg_sum * gamePhase + eg_sum * (24 - gamePhase)) / 24;

        return sum;
    }

    public int Negamax(Board board, int depth, int alpha, int beta, Timer timer, int ply)
    {
        if (board.IsInCheckmate()) return -10000;

        if (ply > 0 && board.IsRepeatedPosition())
        {
            return 0;
        }

        int bestValue = int.MinValue;
        Move[] consider_moves;

        if (depth <= 0)
        {
            consider_moves = board.GetLegalMoves(true);

            bestValue = Eval(board);

            if (bestValue >= beta || timer.MillisecondsElapsedThisTurn > timer.MillisecondsRemaining / 30)
            {
                return bestValue;
            }

            if (alpha < bestValue)
            {
                alpha = bestValue;
            }
        }
        else
        {
            consider_moves = board.GetLegalMoves();
        }

        foreach (Move move in consider_moves)
        {
            board.MakeMove(move);
            int value = -Negamax(board, depth - 1, -beta, -alpha, timer, ply + 1);
            board.UndoMove(move);


            if (bestValue < value)
            {
                bestValue = value;

                if (ply == 0)
                {
                    best_move = move;
                }
            }

            alpha = Math.Max(alpha, value);

            if (alpha >= beta)
            {
                break;
            }
        }

        return bestValue;
    }

    public Move Think(Board board, Timer timer)
    {
        // for (int d = 1;; d++) {
        //     Negamax(board, d, -10000, 10000, timer, 0);

        //     if (d > max_depth) {
        //         break;
        //     }
        // }
        Negamax(board, max_depth, -10000, 10000, timer, 0);

        return best_move;
    }
}