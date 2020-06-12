// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using System.Collections.Generic;

namespace Tobii.XR.GazeModifier
{

    public interface IPercentileRepository
    {
        IList<PercentileData> LoadAll();
    }

    public class PercentileRepository : IPercentileRepository
    {
        private static readonly IList<PercentileData> _all = new List<PercentileData>()
            {
                new PercentileData(0, 0, 0 ,1),

                new PercentileData(1, 0, 0.128139f ,1),
                new PercentileData(1, 10, 0.207933f,1),
                new PercentileData(1, 20, 0.235443f,1),
                new PercentileData(1, 25, 0.321907f,1),

                new PercentileData(2, 0, 0.156792f ,1),
                new PercentileData(2, 10, 0.268569f,1),
                new PercentileData(2, 20, 0.295879f,1),
                new PercentileData(2, 25, 0.414854f,1),

                new PercentileData(3, 0, 0.171788f ,1),
                new PercentileData(3, 10, 0.270936f,1),
                new PercentileData(3, 20, 0.321852f,1),
                new PercentileData(3, 25, 0.477734f,1),

                new PercentileData(4, 0, 0.186288f ,1),
                new PercentileData(4, 10, 0.275646f,1),
                new PercentileData(4, 20, 0.346236f,1),
                new PercentileData(4, 25, 0.47804f ,1),

                new PercentileData(5, 0, 0.191005f ,1),
                new PercentileData(5, 10, 0.289754f,1),
                new PercentileData(5, 20, 0.378245f,1),
                new PercentileData(5, 25, 0.574865f,1),

                new PercentileData(6, 0, 0.202523f ,1),
                new PercentileData(6, 10, 0.29978f ,1),
                new PercentileData(6, 20, 0.385184f,1),
                new PercentileData(6, 25, 0.582905f,1),

                new PercentileData(7, 0, 0.205979f ,1),
                new PercentileData(7, 10, 0.299852f,1),
                new PercentileData(7, 20, 0.396713f,1),
                new PercentileData(7, 25, 0.638268f,1),

                new PercentileData(8, 0, 0.211098f ,1),
                new PercentileData(8, 10, 0.302607f,1),
                new PercentileData(8, 20, 0.400241f,1),
                new PercentileData(8, 25, 0.652696f,1),

                new PercentileData(9, 0, 0.237354f ,1),
                new PercentileData(9, 10, 0.333026f,1),
                new PercentileData(9, 20, 0.40664f ,1),
                new PercentileData(9, 25, 0.659514f,1),

                new PercentileData(10, 0, 0.251758f,1),
                new PercentileData(10, 10, 0.365924f,1f),
                new PercentileData(10, 20, 0.417507f,1f),
                new PercentileData(10, 25, 0.684573f,1f),

                new PercentileData(11, 0, 0.259273f ,1f),
                new PercentileData(11, 10, 0.366881f,1f),
                new PercentileData(11, 20, 0.418995f,1f),
                new PercentileData(11, 25, 0.694556f,1f),

                new PercentileData(12, 0, 0.266884f ,1f),
                new PercentileData(12, 10, 0.370279f,1f),
                new PercentileData(12, 20, 0.436022f,1f),
                new PercentileData(12, 25, 0.731099f,1f),

                new PercentileData(13, 0, 0.274712f ,1f),
                new PercentileData(13, 10, 0.370668f,1f),
                new PercentileData(13, 20, 0.457136f,1f),
                new PercentileData(13, 25, 0.776742f,1f),

                new PercentileData(14, 0, 0.284088f ,1f),
                new PercentileData(14, 10, 0.37675f ,1f),
                new PercentileData(14, 20, 0.493541f,1f),
                new PercentileData(14, 25, 0.780293f,1f),

                new PercentileData(15, 0, 0.288455f ,1f),
                new PercentileData(15, 10, 0.380131f,1f),
                new PercentileData(15, 20, 0.498363f,1f),
                new PercentileData(15, 25, 0.80271f ,1f),

                new PercentileData(16, 0, 0.298895f ,1f),
                new PercentileData(16, 10, 0.39296f ,1f),
                new PercentileData(16, 20, 0.523594f,1f),
                new PercentileData(16, 25, 0.810255f,1f),

                new PercentileData(17, 0, 0.301859f ,1f),
                new PercentileData(17, 10, 0.395058f,1f),
                new PercentileData(17, 20, 0.543775f,1f),
                new PercentileData(17, 25, 0.874086f,1f),

                new PercentileData(18, 0, 0.318224f ,1f),
                new PercentileData(18, 10, 0.414987f,1f),
                new PercentileData(18, 20, 0.586638f,1f),
                new PercentileData(18, 25, 0.889297f,1f),

                new PercentileData(19, 0, 0.324859f ,1f),
                new PercentileData(19, 10, 0.424701f,1f),
                new PercentileData(19, 20, 0.601214f,1f),
                new PercentileData(19, 25, 0.919359f,1f),

                new PercentileData(20, 0, 0.329966f ,1f),
                new PercentileData(20, 10, 0.42899f ,1f),
                new PercentileData(20, 20, 0.613215f,1f),
                new PercentileData(20, 25, 0.931302f,1f),

                new PercentileData(21, 0, 0.334497f ,1f),
                new PercentileData(21, 10, 0.43229f ,1f),
                new PercentileData(21, 20, 0.614884f,1f),
                new PercentileData(21, 25, 0.990864f,1f),

                new PercentileData(22, 0, 0.346694f ,1f),
                new PercentileData(22, 10, 0.4587f  ,1f),
                new PercentileData(22, 20, 0.654126f,1f),
                new PercentileData(22, 25, 0.998277f,1f),

                new PercentileData(23, 0, 0.354084f ,1f),
                new PercentileData(23, 10, 0.462356f,1f),
                new PercentileData(23, 20, 0.675509f,1f),
                new PercentileData(23, 25, 1.011037f,1f),

                new PercentileData(24, 0, 0.359732f ,1f),
                new PercentileData(24, 10, 0.476293f,1f),
                new PercentileData(24, 20, 0.692148f,1f),
                new PercentileData(24, 25, 1.033102f,1f),

                new PercentileData(25, 0, 0.363498f ,1f),
                new PercentileData(25, 10, 0.479365f,1f),
                new PercentileData(25, 20, 0.71354f ,1f),
                new PercentileData(25, 25, 1.052762f,1f),

                new PercentileData(26, 0, 0.380709f ,1f),
                new PercentileData(26, 10, 0.480144f,1f),
                new PercentileData(26, 20, 0.721506f,1f),
                new PercentileData(26, 25, 1.091868f,1f),

                new PercentileData(27, 0, 0.385433f ,1f),
                new PercentileData(27, 10, 0.483924f,1f),
                new PercentileData(27, 20, 0.731422f,1f),
                new PercentileData(27, 25, 1.095337f,1f),

                new PercentileData(28, 0, 0.400536f ,1f),
                new PercentileData(28, 10, 0.489656f,1f),
                new PercentileData(28, 20, 0.733615f,1f),
                new PercentileData(28, 25, 1.098038f,1f),

                new PercentileData(29, 0, 0.408021f ,1f),
                new PercentileData(29, 10, 0.50157f ,1f),
                new PercentileData(29, 20, 0.754409f,1f),
                new PercentileData(29, 25, 1.103191f,1f),

                new PercentileData(30, 0, 0.415131f ,1f),
                new PercentileData(30, 10, 0.501573f,1f),
                new PercentileData(30, 20, 0.76004f ,1f),
                new PercentileData(30, 25, 1.15388f ,1f),

                new PercentileData(31, 0, 0.424371f ,1f),
                new PercentileData(31, 10, 0.508168f,1f),
                new PercentileData(31, 20, 0.772569f,1f),
                new PercentileData(31, 25, 1.196411f,1f),

                new PercentileData(32, 0, 0.433601f ,1f),
                new PercentileData(32, 10, 0.519444f,1f),
                new PercentileData(32, 20, 0.781869f,1f),
                new PercentileData(32, 25, 1.197959f,1f),

                new PercentileData(33, 0, 0.438402f ,1f),
                new PercentileData(33, 10, 0.523689f,1f),
                new PercentileData(33, 20, 0.782092f,1f),
                new PercentileData(33, 25, 1.214959f,1f),

                new PercentileData(34, 0, 0.449213f ,1f),
                new PercentileData(34, 10, 0.53443f ,1f),
                new PercentileData(34, 20, 0.826147f,1f),
                new PercentileData(34, 25, 1.234789f,1f),

                new PercentileData(35, 0, 0.458258f ,1f),
                new PercentileData(35, 10, 0.541323f,1f),
                new PercentileData(35, 20, 0.836376f,1f),
                new PercentileData(35, 25, 1.238723f,1f),

                new PercentileData(36, 0, 0.465874f ,1f),
                new PercentileData(36, 10, 0.544406f,1f),
                new PercentileData(36, 20, 0.845257f,1f),
                new PercentileData(36, 25, 1.255433f,1f),

                new PercentileData(37, 0, 0.471385f ,1f),
                new PercentileData(37, 10, 0.553768f,1f),
                new PercentileData(37, 20, 0.888144f,1f),
                new PercentileData(37, 25, 1.296412f,1f),

                new PercentileData(38, 0, 0.476047f ,1f),
                new PercentileData(38, 10, 0.557871f,1f),
                new PercentileData(38, 20, 0.891753f,1f),
                new PercentileData(38, 25, 1.348228f,1f),

                new PercentileData(39, 0, 0.492372f ,1f),
                new PercentileData(39, 10, 0.589287f,1f),
                new PercentileData(39, 20, 0.892637f,1f),
                new PercentileData(39, 25, 1.363813f,1f),

                new PercentileData(40, 0, 0.497902f ,1f),
                new PercentileData(40, 10, 0.594972f,1f),
                new PercentileData(40, 20, 0.919792f,1f),
                new PercentileData(40, 25, 1.367624f,1f),

                new PercentileData(41, 0, 0.507612f ,1f),
                new PercentileData(41, 10, 0.597109f,1f),
                new PercentileData(41, 20, 0.950868f,1f),
                new PercentileData(41, 25, 1.377068f,1f),

                new PercentileData(42, 0, 0.513668f ,1f),
                new PercentileData(42, 10, 0.603804f,1f),
                new PercentileData(42, 20, 0.970353f,1f),
                new PercentileData(42, 25, 1.395442f,1f),

                new PercentileData(43, 0, 0.521135f ,1f),
                new PercentileData(43, 10, 0.605706f,1f),
                new PercentileData(43, 20, 1.00661f ,1f),
                new PercentileData(43, 25, 1.404149f,1f),

                new PercentileData(44, 0, 0.528717f ,1f),
                new PercentileData(44, 10, 0.629108f,1f),
                new PercentileData(44, 20, 1.018894f,1f),
                new PercentileData(44, 25, 1.407122f,1f),

                new PercentileData(45, 0, 0.541072f ,1f),
                new PercentileData(45, 10, 0.655235f,1f),
                new PercentileData(45, 20, 1.027285f,1f),
                new PercentileData(45, 25, 1.420527f,1f),

                new PercentileData(46, 0, 0.551471f ,1f),
                new PercentileData(46, 10, 0.665772f,1f),
                new PercentileData(46, 20, 1.029136f,1f),
                new PercentileData(46, 25, 1.436592f,1f),

                new PercentileData(47, 0, 0.557727f ,1f),
                new PercentileData(47, 10, 0.680951f,1f),
                new PercentileData(47, 20, 1.035463f,1f),
                new PercentileData(47, 25, 1.441086f,1f),

                new PercentileData(48, 0, 0.560148f ,1f),
                new PercentileData(48, 10, 0.69019f ,1f),
                new PercentileData(48, 20, 1.071724f,1f),
                new PercentileData(48, 25, 1.446203f,1f),

                new PercentileData(49, 0, 0.571652f, 1f),
                new PercentileData(49, 10, 0.695848f,1f),
                new PercentileData(49, 20, 1.073105f,1f),
                new PercentileData(49, 25, 1.473889f,1f),

                new PercentileData(50, 0, 0.573646f, 1f),
                new PercentileData(50, 10, 0.707837f,1f),
                new PercentileData(50, 20, 1.078342f,1f),
                new PercentileData(50, 25, 1.515628f,1f),

                new PercentileData(51, 0, 0.583576f ,1f),
                new PercentileData(51, 10, 0.743103f,1f),
                new PercentileData(51, 20, 1.09067f ,1f),
                new PercentileData(51, 25, 1.524702f,1f),

                new PercentileData(52, 0, 0.59126f  ,1f),
                new PercentileData(52, 10, 0.743375f,1f),
                new PercentileData(52, 20, 1.094185f,1f),
                new PercentileData(52, 25, 1.572785f,1f),

                new PercentileData(53, 0, 0.595631f ,1f),
                new PercentileData(53, 10, 0.75918f ,1f),
                new PercentileData(53, 20, 1.117168f,1f),
                new PercentileData(53, 25, 1.593892f,1f),

                new PercentileData(54, 0, 0.602309f ,1f),
                new PercentileData(54, 10, 0.765564f,1f),
                new PercentileData(54, 20, 1.123839f,1f),
                new PercentileData(54, 25, 1.616138f,1f),

                new PercentileData(55, 0, 0.607935f ,1f),
                new PercentileData(55, 10, 0.800489f,1f),
                new PercentileData(55, 20, 1.138637f,1f),
                new PercentileData(55, 25, 1.644498f,1f),

                new PercentileData(56, 0, 0.61294f  ,1f),
                new PercentileData(56, 10, 0.807969f,1f),
                new PercentileData(56, 20, 1.207632f,1f),
                new PercentileData(56, 25, 1.652612f,1f),

                new PercentileData(57, 0, 0.617185f ,1f),
                new PercentileData(57, 10, 0.823799f,1f),
                new PercentileData(57, 20, 1.211699f,1f),
                new PercentileData(57, 25, 1.690617f,1f),

                new PercentileData(58, 0, 0.626733f ,1f),
                new PercentileData(58, 10, 0.838457f,1f),
                new PercentileData(58, 20, 1.235243f,1f),
                new PercentileData(58, 25, 1.802618f,1f),

                new PercentileData(59, 0, 0.630819f ,1f),
                new PercentileData(59, 10, 0.853762f,1f),
                new PercentileData(59, 20, 1.259121f,1f),
                new PercentileData(59, 25, 1.835148f,1f),

                new PercentileData(60, 0, 0.649302f ,1f),
                new PercentileData(60, 10, 0.912508f,1f),
                new PercentileData(60, 20, 1.287578f,1f),
                new PercentileData(60, 25, 1.836639f,1f),

                new PercentileData(61, 0, 0.656729f ,1f),
                new PercentileData(61, 10, 0.945219f,1f),
                new PercentileData(61, 20, 1.331471f,1f),
                new PercentileData(61, 25, 1.845752f,1f),

                new PercentileData(62, 0, 0.688509f ,1f),
                new PercentileData(62, 10, 0.946334f,1f),
                new PercentileData(62, 20, 1.365019f,1f),
                new PercentileData(62, 25, 1.89396f ,1f),

                new PercentileData(63, 0, 0.693354f ,1f),
                new PercentileData(63, 10, 0.967892f,1f),
                new PercentileData(63, 20, 1.374321f,1f),
                new PercentileData(63, 25, 1.935155f,1f),

                new PercentileData(64, 0, 0.705322f ,1f),
                new PercentileData(64, 10, 0.981615f,1f),
                new PercentileData(64, 20, 1.375474f,1f),
                new PercentileData(64, 25, 1.94382f ,1f),

                new PercentileData(65, 0, 0.72909f  ,1f),
                new PercentileData(65, 10, 1.01286f ,1f),
                new PercentileData(65, 20, 1.391149f,1f),
                new PercentileData(65, 25, 1.96383f ,1f),

                new PercentileData(66, 0, 0.753511f ,1f),
                new PercentileData(66, 10, 1.054194f,1f),
                new PercentileData(66, 20, 1.411104f,1f),
                new PercentileData(66, 25, 1.995594f,1f),

                new PercentileData(67, 0, 0.7663f   ,1f),
                new PercentileData(67, 10, 1.07189f ,1f),
                new PercentileData(67, 20, 1.417259f,1f),
                new PercentileData(67, 25, 2.038918f,1f),

                new PercentileData(68, 0, 0.77629f  ,1f),
                new PercentileData(68, 10, 1.082394f,1f),
                new PercentileData(68, 20, 1.439075f,1f),
                new PercentileData(68, 25, 2.041364f,1f),

                new PercentileData(69, 0, 0.790289f ,1f),
                new PercentileData(69, 10, 1.096333f,1f),
                new PercentileData(69, 20, 1.453917f,1f),
                new PercentileData(69, 25, 2.065498f,1f),

                new PercentileData(70, 0, 0.80413f  ,1f),
                new PercentileData(70, 10, 1.107611f,1f),
                new PercentileData(70, 20, 1.559689f,1f),
                new PercentileData(70, 25, 2.072435f,1f),

                new PercentileData(71, 0, 0.828679f ,1f),
                new PercentileData(71, 10, 1.109694f,1f),
                new PercentileData(71, 20, 1.570577f,1f),
                new PercentileData(71, 25, 2.119462f,1f),

                new PercentileData(72, 0, 0.837762f ,1f),
                new PercentileData(72, 10, 1.113322f,1f),
                new PercentileData(72, 20, 1.572091f,1f),
                new PercentileData(72, 25, 2.14073f ,1f),

                new PercentileData(73, 0, 0.856017f ,1f),
                new PercentileData(73, 10, 1.160137f,1f),
                new PercentileData(73, 20, 1.632134f,1f),
                new PercentileData(73, 25, 2.143802f,1f),

                new PercentileData(74, 0, 0.882012f ,1f),
                new PercentileData(74, 10, 1.17044f ,1f),
                new PercentileData(74, 20, 1.686322f,1f),
                new PercentileData(74, 25, 2.265908f,1f),

                new PercentileData(75, 0, 0.919462f ,1f),
                new PercentileData(75, 10, 1.188337f,1f),
                new PercentileData(75, 20, 1.697997f,1f),
                new PercentileData(75, 25, 2.309494f,1f),

                new PercentileData(76, 0, 0.947244f ,1f),
                new PercentileData(76, 10, 1.221622f,1f),
                new PercentileData(76, 20, 1.734816f,1f),
                new PercentileData(76, 25, 2.314891f,1f),

                new PercentileData(77, 0, 0.961622f ,1f),
                new PercentileData(77, 10, 1.294936f,1f),
                new PercentileData(77, 20, 1.787705f,1f),
                new PercentileData(77, 25, 2.361752f,1f),

                new PercentileData(78, 0, 1.006796f ,1f),
                new PercentileData(78, 10, 1.311459f,1f),
                new PercentileData(78, 20, 1.803479f,1f),
                new PercentileData(78, 25, 2.38631f ,1f),

                new PercentileData(79, 0, 1.037091f ,1f),
                new PercentileData(79, 10, 1.397142f,1f),
                new PercentileData(79, 20, 1.848641f,.999f),
                new PercentileData(79, 25, 2.400855f,.998f),

                new PercentileData(80, 0, 1.061951f ,1f),
                new PercentileData(80, 10, 1.418371f,1f),
                new PercentileData(80, 20, 1.880719f,.995f),
                new PercentileData(80, 25, 2.432212f,.994f),

                new PercentileData(81, 0, 1.083926f ,1f),
                new PercentileData(81, 10, 1.419594f,1f),
                new PercentileData(81, 20, 1.889917f,.991f),
                new PercentileData(81, 25, 2.448526f,.990f),

                new PercentileData(82, 0, 1.10767f  ,1f),
                new PercentileData(82, 10, 1.436315f,1f),
                new PercentileData(82, 20, 1.902617f,.987f),
                new PercentileData(82, 25, 2.534432f,.986f),

                new PercentileData(83, 0, 1.139212f ,1f),
                new PercentileData(83, 10, 1.477079f,1f),
                new PercentileData(83, 20, 1.95818f ,.983f),
                new PercentileData(83, 25, 2.552706f,.982f),

                new PercentileData(84, 0, 1.169689f ,1f),
                new PercentileData(84, 10, 1.502889f,1f),
                new PercentileData(84, 20, 2.01369f ,.979f),
                new PercentileData(84, 25, 2.600981f,.978f),

                new PercentileData(85, 0, 1.230766f ,1f),
                new PercentileData(85, 10, 1.506485f,1f),
                new PercentileData(85, 20, 2.063714f,.977f),
                new PercentileData(85, 25, 2.615962f,.976f),

                new PercentileData(86, 0, 1.28288f  ,1f),
                new PercentileData(86, 10, 1.523306f,1f),
                new PercentileData(86, 20, 2.085586f,.975f),
                new PercentileData(86, 25, 2.666665f,.974f),

                new PercentileData(87, 0, 1.321077f ,1f),
                new PercentileData(87, 10, 1.537287f,1f),
                new PercentileData(87, 20, 2.249905f,.973f),
                new PercentileData(87, 25, 2.685529f,.972f),

                new PercentileData(88, 0, 1.365697f ,1f),
                new PercentileData(88, 10, 1.561968f,1f),
                new PercentileData(88, 20, 2.306258f,.971f),
                new PercentileData(88, 25, 2.770141f,.970f),

                new PercentileData(89, 0, 1.421563f ,1f),
                new PercentileData(89, 10, 1.585145f,.996f),
                new PercentileData(89, 20, 2.319246f,.969f),
                new PercentileData(89, 25, 2.843916f,.968f),

                new PercentileData(90, 0, 1.455829f ,1f),
                new PercentileData(90, 10, 1.588799f,.991f),
                new PercentileData(90, 20, 2.441226f,.967f),
                new PercentileData(90, 25, 2.969066f,.966f),

                new PercentileData(91, 0, 1.486723f ,1f),
                new PercentileData(91, 10, 1.594139f,.986f),
                new PercentileData(91, 20, 2.448212f,.965f),
                new PercentileData(91, 25, 2.984893f,.964f),

                new PercentileData(92, 0, 1.577919f ,1f),
                new PercentileData(92, 10, 1.606521f,.981f),
                new PercentileData(92, 20, 2.494682f,.963f),
                new PercentileData(92, 25, 3.438337f,.962f),

                new PercentileData(93, 0, 1.62077f  ,1f),
                new PercentileData(93, 10, 1.648946f,.976f),
                new PercentileData(93, 20, 3.015885f,.961f),
                new PercentileData(93, 25, 3.5702f  ,.960f),

                new PercentileData(94, 0, 1.746669f ,1f),
                new PercentileData(94, 10, 1.820187f,.971f),
                new PercentileData(94, 20, 3.220106f,.959f),
                new PercentileData(94, 25, 3.852422f,.958f),

                new PercentileData(95, 0, 1.834014f , 1f),
                new PercentileData(95, 10, 1.964536f,.966f),
                new PercentileData(95, 20, 3.222319f,.957f),
                new PercentileData(95, 25, 3.905004f,.956f),

                new PercentileData(96, 0, 1.989665f ,.1f),
                new PercentileData(96, 10, 4.549586f,.946f),
                new PercentileData(96, 20, 4.992804f,.937f),
                new PercentileData(96, 25, 4.678271f,.936f),

                new PercentileData(97, 0, 2.190121f ,.1f),
                new PercentileData(97, 10, 4.58471f ,.926f),
                new PercentileData(97, 20, 5.120141f,.917f),
                new PercentileData(97, 25, 5.695905f,.916f),

                new PercentileData(98, 0, 2.432144f ,.1f),
                new PercentileData(98, 10, 4.616571f,.886f),
                new PercentileData(98, 20, 5.00802f ,.877f),
                new PercentileData(98, 25, 5.808607f,.876f),

                new PercentileData(99, 0, 2.966073f ,.1f),
                new PercentileData(99, 10, 5.278525f,.846f),
                new PercentileData(99, 20, 8.21021f ,.837f),
                new PercentileData(99, 25, 9.22182f ,.836f),

            };
        public IList<PercentileData> LoadAll()
        {
            return _all;
        }
    }
}




