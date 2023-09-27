﻿using ChessChallenge.API;
using System;
using System.Collections;

public class MyBot : IChessBot
{
    long[] raw_mgeg_table = {281479271743489000, 281479271743489000, 309064394990748743, 300620111327921117, 279790451949700103, 299775385749488596, 277538677904507901, 287953247748555729, 273879438782039028, 286264372118029263, 274160905168880606, 282323709560685532, 271627643262338001, 277257250174534610, 281479271743489000, 281479271743489000, 234472568378491831, 298648828710028157, 260931422354670604, 287953462496265175, 268250205538550825, 305123723842749460, 278946069968782365, 291894142235706366, 277820114227168245, 289360652697142240, 275005308625224690, 286827369317073880, 273316269784826853, 281197874075272149, 251924308990755783, 276694076879078353, 273316514593375171, 274442216937554912, 274160991067309019, 289923774449058745, 276975831032726544, 291331110679151590, 280353393312728090, 291894004796031974, 279790477718651906, 291049472491914220, 281479336168981495, 285420037382734834, 282605236075889640, 283449686776939497, 272190584626217939, 277820045504086995, 290486651388953627, 299212233932997651, 289079233557431334, 303997557644854292, 280071978466018316, 286264539625096184, 274723825058251778, 288234821507875796, 271346060911969255, 284012516469507025, 268812790416278487, 282323696673293255, 269094304047432671, 281197844011025313, 276131191351477241, 285982901433205710, 273597972397491188, 298086484350796821, 274723704798381033, 276975916931089438, 277820024032265200, 289642286589346849, 273879331407135704, 281197869781091305, 278945885283353566, 280916304610395109, 277538630658753510, 280071905450787821, 271627613199598570, 283731135981487081, 281197719456777202, 277257039716615094, 263183497042592729, 265716527018935285, 289642041771820001, 279227454747444171, 278946100032439256, 275849797980521426, 276694111239275469, 273034915067069380, 267686993587930049, 268531233834075061, 277538561938555834, 269094243918218189, 281760776784446376, 269375779026043888, 277257301712438194, 283730951299662838,
                             281479271743489000, 281479271743489000, 331582560637682798, 322856660266452131, 307938349056590891, 297242098077926460, 290486574078297069, 280916338971051001, 285138485095236577, 279508912546972647, 282605201714709481, 281479250268586976, 285138480800990194, 285138446440858593, 281479271743489000, 281479271743489000, 265153559884661708, 272753431497212805, 274442362964345830, 278945889577337780, 274723786403742705, 281197758110827455, 276694210025751550, 287671768476287958, 276412696393942017, 285982944385565654, 275005334394176503, 284294008624382930, 269657236821640163, 280916235889214396, 273316278374040537, 275286744943166376, 277538531874505696, 279508908250694608, 279227454750393308, 280634790978520026, 282042187337171943, 280916347559871468, 280634885468849137, 285419964367307754, 279790434768978939, 283449639529939935, 278101559138583538, 285138459325170649, 277538544759669735, 282605132994642893, 275005308622930915, 278945928233288663, 285138489391580151, 284857023004148717, 284575552322733043, 280634859698783211, 283449626645693421, 282605158765102053, 282605184536085481, 282042225991812074, 282323718148981740, 280071871089607645, 280353371836318695, 279508895366382552, 279790396113421290, 278945958297666533, 278946005543224295, 280071841025622996, 278946091443815427, 289079177719710716, 276694283040850961, 297804927768855528, 275849797979669529, 294708745973990385, 282323791164474389, 297523517218423820, 276412842423026711, 290205142052963327, 276975556152984558, 284012619548984301, 275286723469640664, 276975573329511368, 272190477251511229, 280071759419671487, 260649973141865430, 278383111424443351, 278101645038322681, 286264509557834739, 284294094526546935, 287108964554114037, 279227566420657155, 288797762873590763, 276412704984204288, 289079194899514333, 276131234301805565, 287953264927769567, 273879400127923189, 285419938596979671, 266560951947559901, 273597912264475581};
    int[] value_mg = { 82, 237, 365, 477, 1025, 12000 };
    int[] value_eg = { 94, 281, 297, 512, 936, 12000 };

    // value of pieces by game phase
    int[] value_gp = { 0, 1, 1, 2, 4, 0 };

    short Decode(long encoded, int position)
    {
        return (short)((encoded >> (4 - position) * 16 & 0xFFFF) - 1000);
    }

    short table_query(int table, int piece, int index) {
        int ind = 384 * table + piece * 64 + index;
        return Decode(raw_mgeg_table[ind / 4], ind % 4 + 1);
    }

    int depth = 3;

    struct Transposition
    {
        public ulong zobrist;
        public Move move;
        public int eval;

        public Transposition(ulong _zobrist, Move _move, int _eval)
        {
            zobrist = _zobrist;
            move = _move;
            eval = _eval;
        }
    }

    const int TTotal = (1 << 20);
    Transposition[] tt = new Transposition[TTotal];

    int tb_ind(int n)
    {
        return (7 - n / 8) * 8 + (n % 8);
    }

    int Eval(Board board)
    {
        Transposition cur = tt[board.ZobristKey % TTotal];
        if (cur.zobrist == board.ZobristKey)
        {
            return cur.eval;
        }

        int gamePhase = 0, mg_sum = 0, eg_sum = 0;

        PieceList[] all_pl = board.GetAllPieceLists();

        foreach (PieceList pl in all_pl)
        {
            for (int i = 0; i < pl.Count; i++)
            {
                Piece p = pl.GetPiece(i);

                int p_type_ind = (int)p.PieceType - 1;

                gamePhase += value_gp[p_type_ind];

                // if white flip board, otherwise dont (because arrays listed in POV of white)
                int ind = p.IsWhite ? tb_ind(p.Square.Index) : p.Square.Index;

                int neg = board.IsWhiteToMove == p.IsWhite ? 1 : -1;

                mg_sum += table_query(0, p_type_ind, ind) + value_mg[p_type_ind] * neg;
                eg_sum += table_query(1, p_type_ind, ind) + value_eg[p_type_ind] * neg;
            }
        }

        gamePhase = Math.Min(gamePhase, 12);

        int sum = (mg_sum * gamePhase + eg_sum * (12 - gamePhase)) / 12;

        cur = new Transposition(board.ZobristKey, Move.NullMove, sum);

        return sum;
    }

    public int Negamax(Board board, int depth, int alpha, int beta)
    {
        if (board.IsInCheckmate())
        {
            return -100000;
        }

        if (depth == 0)
        {
            return Eval(board);
        }

        int bestValue = int.MinValue;

        foreach (Move move in board.GetLegalMoves())
        {
            board.MakeMove(move);
            int value = -Negamax(board, depth - 1, -beta, -alpha);
            board.UndoMove(move);

            bestValue = Math.Max(bestValue, value);
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
        Move[] allMoves = board.GetLegalMoves();

        Move moveToPlay = allMoves[0];

        int best_eval = int.MinValue;

        foreach (Move move in allMoves)
        {
            board.MakeMove(move);
            int move_eval = -Negamax(board, depth, int.MinValue, int.MaxValue);
            board.UndoMove(move);

            if (move_eval > best_eval)
            {
                moveToPlay = move;
                best_eval = move_eval;
            }
        }

        return moveToPlay;
    }
}