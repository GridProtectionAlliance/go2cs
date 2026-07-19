// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math;

using math = math_package;
using testing = testing_package;

partial class cmplx_package {

// The higher-precision values in vc26 were used to derive the
// input arguments vc (see also comment below). For reference
// only (do not delete).
internal static slice<complex128> vc26 = new complex128[]{
    (4.97901192488367350108546816D + 7.73887247457810456552351752D.i()),
    (7.73887247457810456552351752D - 0.27688005719200159404635997D.i()),
    (-0.27688005719200159404635997D - 5.01060361827107492160848778D.i()),
    (-5.01060361827107492160848778D + 9.63629370719841737980004837D.i()),
    (9.63629370719841737980004837D + 2.92637723924396464525443662D.i()),
    (2.92637723924396464525443662D + 5.22908343145930665230025625D.i()),
    (5.22908343145930665230025625D + 2.72793991043601025126008608D.i()),
    (2.72793991043601025126008608D + 1.82530809168085506044576505D.i()),
    (1.82530809168085506044576505D - 8.68592476857560136238589621D.i()),
    (-8.68592476857560136238589621D + 4.97901192488367350108546816D.i())
}.slice();

internal static slice<complex128> vc = new complex128[]{
    (4.9790119248836735e+00D + 7.7388724745781045e+00D.i()),
    (7.7388724745781045e+00D - 2.7688005719200159e-01D.i()),
    (-2.7688005719200159e-01D - 5.0106036182710749e+00D.i()),
    (-5.0106036182710749e+00D + 9.6362937071984173e+00D.i()),
    (9.6362937071984173e+00D + 2.9263772392439646e+00D.i()),
    (2.9263772392439646e+00D + 5.2290834314593066e+00D.i()),
    (5.2290834314593066e+00D + 2.7279399104360102e+00D.i()),
    (2.7279399104360102e+00D + 1.8253080916808550e+00D.i()),
    (1.8253080916808550e+00D - 8.6859247685756013e+00D.i()),
    (-8.6859247685756013e+00D + 4.9790119248836735e+00D.i())
}.slice();

// The expected results below were computed by the high precision calculators
// at https://keisan.casio.com/.  More exact input values (array vc[], above)
// were obtained by printing them with "%.26f".  The answers were calculated
// to 26 digits (by using the "Digit number" drop-down control of each
// calculator).
internal static slice<float64> abs = new float64[]{
    9.2022120669932650313380972e+00D,
    7.7438239742296106616261394e+00D,
    5.0182478202557746902556648e+00D,
    1.0861137372799545160704002e+01D,
    1.0070841084922199607011905e+01D,
    5.9922447613166942183705192e+00D,
    5.8978784056736762299945176e+00D,
    3.2822866700678709020367184e+00D,
    8.8756430028990417290744307e+00D,
    1.0011785496777731986390856e+01D
}.slice();

internal static slice<complex128> acos = new complex128[]{
    (1.0017679804707456328694569D - 2.9138232718554953784519807D.i()),
    (0.03606427612041407369636057D + 2.7358584434576260925091256D.i()),
    (1.6249365462333796703711823D + 2.3159537454335901187730929D.i()),
    (2.0485650849650740120660391D - 3.0795576791204117911123886D.i()),
    (0.29621132089073067282488147D - 3.0007392508200622519398814D.i()),
    (1.0664555914934156601503632D - 2.4872865024796011364747111D.i()),
    (0.48681307452231387690013905D - 2.463655912283054555225301D.i()),
    (0.6116977071277574248407752D - 1.8734458851737055262693056D.i()),
    (1.3649311280370181331184214D + 2.8793528632328795424123832D.i()),
    (2.6189310485682988308904501D - 2.9956543302898767795858704D.i())
}.slice();

internal static slice<complex128> acosh = new complex128[]{
    (2.9138232718554953784519807D + 1.0017679804707456328694569D.i()),
    (2.7358584434576260925091256D - 0.03606427612041407369636057D.i()),
    (2.3159537454335901187730929D - 1.6249365462333796703711823D.i()),
    (3.0795576791204117911123886D + 2.0485650849650740120660391D.i()),
    (3.0007392508200622519398814D + 0.29621132089073067282488147D.i()),
    (2.4872865024796011364747111D + 1.0664555914934156601503632D.i()),
    (2.463655912283054555225301D + 0.48681307452231387690013905D.i()),
    (1.8734458851737055262693056D + 0.6116977071277574248407752D.i()),
    (2.8793528632328795424123832D - 1.3649311280370181331184214D.i()),
    (2.9956543302898767795858704D + 2.6189310485682988308904501D.i())
}.slice();

internal static slice<complex128> asin = new complex128[]{
    (0.56902834632415098636186476D + 2.9138232718554953784519807D.i()),
    (1.5347320506744825455349611D - 2.7358584434576260925091256D.i()),
    (-0.054140219438483051139860579D - 2.3159537454335901187730929D.i()),
    (-0.47776875817017739283471738D + 3.0795576791204117911123886D.i()),
    (1.2745850059041659464064402D + 3.0007392508200622519398814D.i()),
    (0.50434073530148095908095852D + 2.4872865024796011364747111D.i()),
    (1.0839832522725827423311826D + 2.463655912283054555225301D.i()),
    (0.9590986196671391943905465D + 1.8734458851737055262693056D.i()),
    (0.20586519875787848611290031D - 2.8793528632328795424123832D.i()),
    (-1.0481347217734022116591284D + 2.9956543302898767795858704D.i())
}.slice();

internal static slice<complex128> asinh = new complex128[]{
    (2.9113760469415295679342185D + 0.99639459545704326759805893D.i()),
    (2.7441755423994259061579029D - 0.035468308789000500601119392D.i()),
    (-2.2962136462520690506126678D - 1.5144663565690151885726707D.i()),
    (-3.0771233459295725965402455D + 1.0895577967194013849422294D.i()),
    (3.0048366100923647417557027D + 0.29346979169819220036454168D.i()),
    (2.4800059370795363157364643D + 1.0545868606049165710424232D.i()),
    (2.4718773838309585611141821D + 0.47502344364250803363708842D.i()),
    (1.8910743588080159144378396D + 0.56882925572563602341139174D.i()),
    (2.8735426423367341878069406D - 1.362376149648891420997548D.i()),
    (-2.9981750586172477217567878D + 0.5183571985225367505624207D.i())
}.slice();

internal static slice<complex128> atan = new complex128[]{
    (1.5115747079332741358607654D + 0.091324403603954494382276776D.i()),
    (1.4424504323482602560806727D - 0.0045416132642803911503770933D.i()),
    (-1.5593488703630532674484026D - 0.20163295409248362456446431D.i()),
    (-1.5280619472445889867794105D + 0.081721556230672003746956324D.i()),
    (1.4759909163240799678221039D + 0.028602969320691644358773586D.i()),
    (1.4877353772046548932715555D + 0.14566877153207281663773599D.i()),
    (1.4206983927779191889826D + 0.076830486127880702249439993D.i()),
    (1.3162236060498933364869556D + 0.16031313000467530644933363D.i()),
    (1.5473450684303703578810093D - 0.11064907507939082484935782D.i()),
    (-1.4841462340185253987375812D + 0.049341850305024399493142411D.i())
}.slice();

internal static slice<complex128> atanh = new complex128[]{
    (0.058375027938968509064640438D + 1.4793488495105334458167782D.i()),
    (0.12977343497790381229915667D - 1.5661009410463561327262499D.i()),
    (-0.010576456067347252072200088D - 1.3743698658402284549750563D.i()),
    (-0.042218595678688358882784918D + 1.4891433968166405606692604D.i()),
    (0.095218997991316722061828397D + 1.5416884098777110330499698D.i()),
    (0.079965459366890323857556487D + 1.4252510353873192700350435D.i()),
    (0.15051245471980726221708301D + 1.4907432533016303804884461D.i()),
    (0.25082072933993987714470373D + 1.392057665392187516442986D.i()),
    (0.022896108815797135846276662D - 1.4609224989282864208963021D.i()),
    (-0.08665624101841876130537396D + 1.5207902036935093480142159D.i())
}.slice();

internal static slice<complex128> conj = new complex128[]{
    (4.9790119248836735e+00D - 7.7388724745781045e+00D.i()),
    (7.7388724745781045e+00D + 2.7688005719200159e-01D.i()),
    (-2.7688005719200159e-01D + 5.0106036182710749e+00D.i()),
    (-5.0106036182710749e+00D - 9.6362937071984173e+00D.i()),
    (9.6362937071984173e+00D - 2.9263772392439646e+00D.i()),
    (2.9263772392439646e+00D - 5.2290834314593066e+00D.i()),
    (5.2290834314593066e+00D - 2.7279399104360102e+00D.i()),
    (2.7279399104360102e+00D - 1.8253080916808550e+00D.i()),
    (1.8253080916808550e+00D + 8.6859247685756013e+00D.i()),
    (-8.6859247685756013e+00D - 4.9790119248836735e+00D.i())
}.slice();

internal static slice<complex128> cos = new complex128[]{
    (3.024540920601483938336569e+02D + 1.1073797572517071650045357e+03D.i()),
    (1.192858682649064973252758e-01D + 2.7857554122333065540970207e-01D.i()),
    (7.2144394304528306603857962e+01D - 2.0500129667076044169954205e+01D.i()),
    (2.24921952538403984190541e+03D - 7.317363745602773587049329e+03D.i()),
    (-9.148222970032421760015498e+00D + 1.953124661113563541862227e+00D.i()),
    (-9.116081175857732248227078e+01D - 1.992669213569952232487371e+01D.i()),
    (3.795639179042704640002918e+00D + 6.623513350981458399309662e+00D.i()),
    (-2.9144840732498869560679084e+00D - 1.214620271628002917638748e+00D.i()),
    (-7.45123482501299743872481e+02D + 2.8641692314488080814066734e+03D.i()),
    (-5.371977967039319076416747e+01D + 4.893348341339375830564624e+01D.i())
}.slice();

internal static slice<complex128> cosh = new complex128[]{
    (8.34638383523018249366948e+00D + 7.2181057886425846415112064e+01D.i()),
    (1.10421967379919366952251e+03D - 3.1379638689277575379469861e+02D.i()),
    (3.051485206773701584738512e-01D - 2.6805384730105297848044485e-01D.i()),
    (-7.33294728684187933370938e+01D + 1.574445942284918251038144e+01D.i()),
    (-7.478643293945957535757355e+03D + 1.6348382209913353929473321e+03D.i()),
    (4.622316522966235701630926e+00D - 8.088695185566375256093098e+00D.i()),
    (-8.544333183278877406197712e+01D + 3.7505836120128166455231717e+01D.i()),
    (-1.934457815021493925115198e+00D + 7.3725859611767228178358673e+00D.i()),
    (-2.352958770061749348353548e+00D - 2.034982010440878358915409e+00D.i()),
    (7.79756457532134748165069e+02D + 2.8549350716819176560377717e+03D.i())
}.slice();

internal static slice<complex128> exp = new complex128[]{
    (1.669197736864670815125146e+01D + 1.4436895109507663689174096e+02D.i()),
    (2.2084389286252583447276212e+03D - 6.2759289284909211238261917e+02D.i()),
    (2.227538273122775173434327e-01D + 7.2468284028334191250470034e-01D.i()),
    (-6.5182985958153548997881627e-03D - 1.39965837915193860879044e-03D.i()),
    (-1.4957286524084015746110777e+04D + 3.269676455931135688988042e+03D.i()),
    (9.218158701983105935659273e+00D - 1.6223985291084956009304582e+01D.i()),
    (-1.7088175716853040841444505e+02D + 7.501382609870410713795546e+01D.i()),
    (-3.852461315830959613132505e+00D + 1.4808420423156073221970892e+01D.i()),
    (-4.586775503301407379786695e+00D - 4.178501081246873415144744e+00D.i()),
    (4.451337963005453491095747e-05D - 1.62977574205442915935263e-04D.i())
}.slice();

internal static slice<complex128> log = new complex128[]{
    (2.2194438972179194425697051e+00D + 9.9909115046919291062461269e-01D.i()),
    (2.0468956191154167256337289e+00D - 3.5762575021856971295156489e-02D.i()),
    (1.6130808329853860438751244e+00D - 1.6259990074019058442232221e+00D.i()),
    (2.3851910394823008710032651e+00D + 2.0502936359659111755031062e+00D.i()),
    (2.3096442270679923004800651e+00D + 2.9483213155446756211881774e-01D.i()),
    (1.7904660933974656106951860e+00D + 1.0605860367252556281902109e+00D.i()),
    (1.7745926939841751666177512e+00D + 4.8084556083358307819310911e-01D.i()),
    (1.1885403350045342425648780e+00D + 5.8969634164776659423195222e-01D.i()),
    (2.1833107837679082586772505e+00D - 1.3636647724582455028314573e+00D.i()),
    (2.3037629487273259170991671e+00D + 2.6210913895386013290915234e+00D.i())
}.slice();

internal static slice<complex128> log10 = new complex128[]{
    (9.6389223745559042474184943e-01D + 4.338997735671419492599631e-01D.i()),
    (8.8895547241376579493490892e-01D - 1.5531488990643548254864806e-02D.i()),
    (7.0055210462945412305244578e-01D - 7.0616239649481243222248404e-01D.i()),
    (1.0358753067322445311676952e+00D + 8.9043121238134980156490909e-01D.i()),
    (1.003065742975330237172029e+00D + 1.2804396782187887479857811e-01D.i()),
    (7.7758954439739162532085157e-01D + 4.6060666333341810869055108e-01D.i()),
    (7.7069581462315327037689152e-01D + 2.0882857371769952195512475e-01D.i()),
    (5.1617650901191156135137239e-01D + 2.5610186717615977620363299e-01D.i()),
    (9.4819982567026639742663212e-01D - 5.9223208584446952284914289e-01D.i()),
    (1.0005115362454417135973429e+00D + 1.1383255270407412817250921e+00D.i())
}.slice();

[GoType] partial struct ff {
    internal float64 r, theta;
}

internal static slice<ff> polar = new ff[]{
    new(9.2022120669932650313380972e+00D, 9.9909115046919291062461269e-01D),
    new(7.7438239742296106616261394e+00D, -3.5762575021856971295156489e-02D),
    new(5.0182478202557746902556648e+00D, -1.6259990074019058442232221e+00D),
    new(1.0861137372799545160704002e+01D, 2.0502936359659111755031062e+00D),
    new(1.0070841084922199607011905e+01D, 2.9483213155446756211881774e-01D),
    new(5.9922447613166942183705192e+00D, 1.0605860367252556281902109e+00D),
    new(5.8978784056736762299945176e+00D, 4.8084556083358307819310911e-01D),
    new(3.2822866700678709020367184e+00D, 5.8969634164776659423195222e-01D),
    new(8.8756430028990417290744307e+00D, -1.3636647724582455028314573e+00D),
    new(1.0011785496777731986390856e+01D, 2.6210913895386013290915234e+00D)
}.slice();

internal static slice<complex128> pow = new complex128[]{
    (-2.499956739197529585028819e+00D + 1.759751724335650228957144e+00D.i()),
    (7.357094338218116311191939e+04D - 5.089973412479151648145882e+04D.i()),
    (1.320777296067768517259592e+01D - 3.165621914333901498921986e+01D.i()),
    (-3.123287828297300934072149e-07D - 1.9849567521490553032502223e-7D.i()),
    (8.0622651468477229614813e+04D - 7.80028727944573092944363e+04D.i()),
    (-1.0268824572103165858577141e+00D - 4.716844738244989776610672e-01D.i()),
    (-4.35953819012244175753187e+01D + 2.2036445974645306917648585e+02D.i()),
    (8.3556092283250594950239e-01D - 1.2261571947167240272593282e+01D.i()),
    (1.582292972120769306069625e+03D + 1.273564263524278244782512e+04D.i()),
    (6.592208301642122149025369e-08D + 2.584887236651661903526389e-08D.i())
}.slice();

internal static slice<complex128> sin = new complex128[]{
    (-1.1073801774240233539648544e+03D + 3.024539773002502192425231e+02D.i()),
    (1.0317037521400759359744682e+00D - 3.2208979799929570242818e-02D.i()),
    (-2.0501952097271429804261058e+01D - 7.2137981348240798841800967e+01D.i()),
    (7.3173638080346338642193078e+03D + 2.249219506193664342566248e+03D.i()),
    (-1.964375633631808177565226e+00D - 9.0958264713870404464159683e+00D.i()),
    (1.992783647158514838337674e+01D - 9.11555769410191350416942e+01D.i()),
    (-6.680335650741921444300349e+00D + 3.763353833142432513086117e+00D.i()),
    (1.2794028166657459148245993e+00D - 2.7669092099795781155109602e+00D.i()),
    (2.8641693949535259594188879e+03D + 7.451234399649871202841615e+02D.i()),
    (-4.893811726244659135553033e+01D - 5.371469305562194635957655e+01D.i())
}.slice();

internal static slice<complex128> sinh = new complex128[]{
    (8.34559353341652565758198e+00D + 7.2187893208650790476628899e+01D.i()),
    (1.1042192548260646752051112e+03D - 3.1379650595631635858792056e+02D.i()),
    (-8.239469336509264113041849e-02D + 9.9273668758439489098514519e-01D.i()),
    (7.332295456982297798219401e+01D - 1.574585908122833444899023e+01D.i()),
    (-7.4786432301380582103534216e+03D + 1.63483823493980029604071e+03D.i()),
    (4.595842179016870234028347e+00D - 8.135290105518580753211484e+00D.i()),
    (-8.543842533574163435246793e+01D + 3.750798997857594068272375e+01D.i()),
    (-1.918003500809465688017307e+00D + 7.4358344619793504041350251e+00D.i()),
    (-2.233816733239658031433147e+00D - 2.143519070805995056229335e+00D.i()),
    (-7.797564130187551181105341e+02D - 2.8549352346594918614806877e+03D.i())
}.slice();

internal static slice<complex128> sqrt = new complex128[]{
    (2.6628203086086130543813948e+00D + 1.4531345674282185229796902e+00D.i()),
    (2.7823278427251986247149295e+00D - 4.9756907317005224529115567e-02D.i()),
    (1.5397025302089642757361015e+00D - 1.6271336573016637535695727e+00D.i()),
    (1.7103411581506875260277898e+00D + 2.8170677122737589676157029e+00D.i()),
    (3.1390392472953103383607947e+00D + 4.6612625849858653248980849e-01D.i()),
    (2.1117080764822417640789287e+00D + 1.2381170223514273234967850e+00D.i()),
    (2.3587032281672256703926939e+00D + 5.7827111903257349935720172e-01D.i()),
    (1.7335262588873410476661577e+00D + 5.2647258220721269141550382e-01D.i()),
    (2.3131094974708716531499282e+00D - 1.8775429304303785570775490e+00D.i()),
    (8.1420535745048086240947359e-01D + 3.0575897587277248522656113e+00D.i())
}.slice();

internal static slice<complex128> tan = new complex128[]{
    (-1.928757919086441129134525e-07D + 1.0000003267499169073251826e+00D.i()),
    (1.242412685364183792138948e+00D - 3.17149693883133370106696e+00D.i()),
    (-4.6745126251587795225571826e-05D - 9.9992439225263959286114298e-01D.i()),
    (4.792363401193648192887116e-09D + 1.0000000070589333451557723e+00D.i()),
    (2.345740824080089140287315e-03D + 9.947733046570988661022763e-01D.i()),
    (-2.396030789494815566088809e-05D + 9.9994781345418591429826779e-01D.i()),
    (-7.370204836644931340905303e-03D + 1.0043553413417138987717748e+00D.i()),
    (-3.691803847992048527007457e-02D + 9.6475071993469548066328894e-01D.i()),
    (-2.781955256713729368401878e-08D - 1.000000049848910609006646e+00D.i()),
    (9.4281590064030478879791249e-05D + 9.9999119340863718183758545e-01D.i())
}.slice();

internal static slice<complex128> tanh = new complex128[]{
    (1.0000921981225144748819918e+00D + 2.160986245871518020231507e-05D.i()),
    (9.9999967727531993209562591e-01D - 1.9953763222959658873657676e-07D.i()),
    (-1.765485739548037260789686e+00D + 1.7024216325552852445168471e+00D.i()),
    (-9.999189442732736452807108e-01D + 3.64906070494473701938098e-05D.i()),
    (9.9999999224622333738729767e-01D - 3.560088949517914774813046e-09D.i()),
    (1.0029324933367326862499343e+00D - 4.948790309797102353137528e-03D.i()),
    (9.9996113064788012488693567e-01D - 4.226995742097032481451259e-05D.i()),
    (1.0074784189316340029873945e+00D - 4.194050814891697808029407e-03D.i()),
    (9.9385534229718327109131502e-01D + 5.144217985914355502713437e-02D.i()),
    (-1.0000000491604982429364892e+00D - 2.901873195374433112227349e-08D.i())
}.slice();

// huge values along the real axis for testing reducePi in Tan
internal static slice<complex128> hugeIn = new complex128[]{
    (1 << (int)(28)),
    (1 << (int)(29)),
    (1 << (int)(30)),
    34359738368D,
    -1329227995784915872903807060280344576D,
    1766847064778384329583297500742918515827483896875618958121606201292619776D,
    2037035976334486086268445688409378161051468393665936250636140449354381299763336706183397376D,
    -3121748550315992231381597229793166305748598142664971150859156959625371738819765620120306103063491971159826931121406622895447975679288285306290176D,
    1891969788213177603238151163393619335935685080427297832880571827617792D,
    -2514859209672213815954130451185339640703892284324672462185344991442198389642370554817310158862574988296192D
}.slice();

// Results for tanHuge[i] calculated with https://github.com/robpike/ivy
// using 4096 bits of working precision.
internal static slice<complex128> tanHuge = new complex128[]{
    5.95641897939639421D,
    -0.34551069233430392D,
    -0.78469661331920043D,
    0.84276385870875983D,
    0.40806638884180424D,
    -0.37603456702698076D,
    4.60901287677810962D,
    3.39135965054779932D,
    -6.76813854009065030D,
    -0.76417695016604922D
}.slice();

// special cases conform to C99 standard appendix G.6 Complex arithmetic
internal static float64 inf = math.Inf(1);
internal static float64 nan = math.NaN();

internal static slice<complex128> vcAbsSC = new complex128[]{
    NaN()
}.slice();

internal static slice<float64> absSC = new float64[]{
    math.NaN()
}.slice();

// G.6.1.1
// imaginary sign unspecified
// imaginary sign unspecified

[GoType("dyn")] partial struct acosSCᴛ1 {
    internal complex128 @in, want;
}
internal static slice<acosSCᴛ1> acosSC = new acosSCᴛ1[]{
    new(complex(zero, zero),
        complex(math.Pi / 2D, -zero)),
    new(complex(-zero, zero),
        complex(math.Pi / 2D, -zero)),
    new(complex(zero, nan),
        complex(math.Pi / 2D, nan)),
    new(complex(-zero, nan),
        complex(math.Pi / 2D, nan)),
    new(complex(1.0D, inf),
        complex(math.Pi / 2D, -inf)),
    new(complex(1.0D, nan),
        NaN()),
    new(complex(-inf, 1.0D),
        complex(math.Pi, -inf)),
    new(complex(inf, 1.0D),
        complex(0.0D, -inf)),
    new(complex(-inf, inf),
        complex(3D * math.Pi / 4D, -inf)),
    new(complex(inf, inf),
        complex(math.Pi / 4D, -inf)),
    new(complex(inf, nan),
        complex(nan, -inf)),
    new(complex(-inf, nan),
        complex(nan, inf)),
    new(complex(nan, 1.0D),
        NaN()),
    new(complex(nan, inf),
        complex(nan, -inf)),
    new(NaN(),
        NaN())
}.slice();

// G.6.2.1

[GoType("dyn")] partial struct acoshSCᴛ1 {
    internal complex128 @in, want;
}
internal static slice<acoshSCᴛ1> acoshSC = new acoshSCᴛ1[]{
    new(complex(zero, zero),
        complex(zero, math.Pi / 2D)),
    new(complex(-zero, zero),
        complex(zero, math.Pi / 2D)),
    new(complex(1.0D, inf),
        complex(inf, math.Pi / 2D)),
    new(complex(1.0D, nan),
        NaN()),
    new(complex(-inf, 1.0D),
        complex(inf, math.Pi)),
    new(complex(inf, 1.0D),
        complex(inf, zero)),
    new(complex(-inf, inf),
        complex(inf, 3D * math.Pi / 4D)),
    new(complex(inf, inf),
        complex(inf, math.Pi / 4D)),
    new(complex(inf, nan),
        complex(inf, nan)),
    new(complex(-inf, nan),
        complex(inf, nan)),
    new(complex(nan, 1.0D),
        NaN()),
    new(complex(nan, inf),
        complex(inf, nan)),
    new(NaN(),
        NaN())
}.slice();

// Derived from Asin(z) = -i * Asinh(i * z), G.6 #7
// imaginary sign unspecified

[GoType("dyn")] partial struct asinSCᴛ1 {
    internal complex128 @in, want;
}
internal static slice<asinSCᴛ1> asinSC = new asinSCᴛ1[]{
    new(complex(zero, zero),
        complex(zero, zero)),
    new(complex(1.0D, inf),
        complex(0, inf)),
    new(complex(1.0D, nan),
        NaN()),
    new(complex(inf, 1),
        complex(math.Pi / 2D, inf)),
    new(complex(inf, inf),
        complex(math.Pi / 4D, inf)),
    new(complex(inf, nan),
        complex(nan, inf)),
    new(complex(nan, zero),
        NaN()),
    new(complex(nan, 1),
        NaN()),
    new(complex(nan, inf),
        complex(nan, inf)),
    new(NaN(),
        NaN())
}.slice();

// G.6.2.2
// sign of real part unspecified

[GoType("dyn")] partial struct asinhSCᴛ1 {
    internal complex128 @in, want;
}
internal static slice<asinhSCᴛ1> asinhSC = new asinhSCᴛ1[]{
    new(complex(zero, zero),
        complex(zero, zero)),
    new(complex(1.0D, inf),
        complex(inf, math.Pi / 2D)),
    new(complex(1.0D, nan),
        NaN()),
    new(complex(inf, 1.0D),
        complex(inf, zero)),
    new(complex(inf, inf),
        complex(inf, math.Pi / 4D)),
    new(complex(inf, nan),
        complex(inf, nan)),
    new(complex(nan, zero),
        complex(nan, zero)),
    new(complex(nan, 1.0D),
        NaN()),
    new(complex(nan, inf),
        complex(inf, nan)),
    new(NaN(),
        NaN())
}.slice();

// Derived from Atan(z) = -i * Atanh(i * z), G.6 #7

[GoType("dyn")] partial struct atanSCᴛ1 {
    internal complex128 @in, want;
}
internal static slice<atanSCᴛ1> atanSC = new atanSCᴛ1[]{
    new(complex(0, zero),
        complex(0, zero)),
    new(complex(0, nan),
        NaN()),
    new(complex(1.0D, zero),
        complex(math.Pi / 4D, zero)),
    new(complex(1.0D, inf),
        complex(math.Pi / 2D, zero)),
    new(complex(1.0D, nan),
        NaN()),
    new(complex(inf, 1),
        complex(math.Pi / 2D, zero)),
    new(complex(inf, inf),
        complex(math.Pi / 2D, zero)),
    new(complex(inf, nan),
        complex(math.Pi / 2D, zero)),
    new(complex(nan, 1),
        NaN()),
    new(complex(nan, inf),
        complex(nan, zero)),
    new(NaN(),
        NaN())
}.slice();

// G.6.2.3
// sign of real part not specified.

[GoType("dyn")] partial struct atanhSCᴛ1 {
    internal complex128 @in, want;
}
internal static slice<atanhSCᴛ1> atanhSC = new atanhSCᴛ1[]{
    new(complex(zero, zero),
        complex(zero, zero)),
    new(complex(zero, nan),
        complex(zero, nan)),
    new(complex(1.0D, zero),
        complex(inf, zero)),
    new(complex(1.0D, inf),
        complex(0D, math.Pi / 2D)),
    new(complex(1.0D, nan),
        NaN()),
    new(complex(inf, 1.0D),
        complex(zero, math.Pi / 2D)),
    new(complex(inf, inf),
        complex(zero, math.Pi / 2D)),
    new(complex(inf, nan),
        complex(0, nan)),
    new(complex(nan, 1.0D),
        NaN()),
    new(complex(nan, inf),
        complex(zero, math.Pi / 2D)),
    new(NaN(),
        NaN())
}.slice();

internal static slice<complex128> vcConjSC = new complex128[]{
    NaN()
}.slice();

internal static slice<complex128> conjSC = new complex128[]{
    NaN()
}.slice();

// Derived from Cos(z) = Cosh(i * z), G.6 #7
// imaginary sign unspecified
// real sign unspecified
// imaginary sign unspecified

[GoType("dyn")] partial struct cosSCᴛ1 {
    internal complex128 @in, want;
}
internal static slice<cosSCᴛ1> cosSC = new cosSCᴛ1[]{
    new(complex(zero, zero),
        complex(1.0D, -zero)),
    new(complex(zero, inf),
        complex(inf, -zero)),
    new(complex(zero, nan),
        complex(nan, zero)),
    new(complex(1.0D, inf),
        complex(inf, -inf)),
    new(complex(1.0D, nan),
        NaN()),
    new(complex(inf, zero),
        complex(nan, -zero)),
    new(complex(inf, 1.0D),
        NaN()),
    new(complex(inf, inf),
        complex(inf, nan)),
    new(complex(inf, nan),
        NaN()),
    new(complex(nan, zero),
        complex(nan, -zero)),
    new(complex(nan, 1.0D),
        NaN()),
    new(complex(nan, inf),
        complex(inf, nan)),
    new(NaN(),
        NaN())
}.slice();

// G.6.2.4
// imaginary sign unspecified
// imaginary sign unspecified
// +inf  cis(y)
// real sign unspecified
// imaginary sign unspecified

[GoType("dyn")] partial struct coshSCᴛ1 {
    internal complex128 @in, want;
}
internal static slice<coshSCᴛ1> coshSC = new coshSCᴛ1[]{
    new(complex(zero, zero),
        complex(1.0D, zero)),
    new(complex(zero, inf),
        complex(nan, zero)),
    new(complex(zero, nan),
        complex(nan, zero)),
    new(complex(1.0D, inf),
        NaN()),
    new(complex(1.0D, nan),
        NaN()),
    new(complex(inf, zero),
        complex(inf, zero)),
    new(complex(inf, 1.0D),
        complex(inf * math.Cos(1.0D), inf * math.Sin(1.0D))),
    new(complex(inf, inf),
        complex(inf, nan)),
    new(complex(inf, nan),
        complex(inf, nan)),
    new(complex(nan, zero),
        complex(nan, zero)),
    new(complex(nan, 1.0D),
        NaN()),
    new(complex(nan, inf),
        NaN()),
    new(NaN(),
        NaN())
}.slice();

// G.6.3.1
// +0 cis(y)
// +inf  cis(y)
// real and imaginary sign unspecified
// real sign unspecified
// real and imaginary sign unspecified
// real sign unspecified

[GoType("dyn")] partial struct expSCᴛ1 {
    internal complex128 @in, want;
}
internal static slice<expSCᴛ1> expSC = new expSCᴛ1[]{
    new(complex(zero, zero),
        complex(1.0D, zero)),
    new(complex(-zero, zero),
        complex(1.0D, zero)),
    new(complex(1.0D, inf),
        NaN()),
    new(complex(1.0D, nan),
        NaN()),
    new(complex(inf, zero),
        complex(inf, zero)),
    new(complex(-inf, 1.0D),
        complex(math.Copysign(0.0D, math.Cos(1.0D)), math.Copysign(0.0D, math.Sin(1.0D)))),
    new(complex(inf, 1.0D),
        complex(inf * math.Cos(1.0D), inf * math.Sin(1.0D))),
    new(complex(-inf, inf),
        complex(zero, zero)),
    new(complex(inf, inf),
        complex(inf, nan)),
    new(complex(-inf, nan),
        complex(zero, zero)),
    new(complex(inf, nan),
        complex(inf, nan)),
    new(complex(nan, zero),
        complex(nan, zero)),
    new(complex(nan, 1.0D),
        NaN()),
    new(complex(nan, inf),
        NaN()),
    new(NaN(),
        NaN())
}.slice();

internal static slice<complex128> vcIsNaNSC = new complex128[]{
    complex(math.Inf(-1), math.Inf(-1)),
    complex(math.Inf(-1), math.NaN()),
    complex(math.NaN(), math.Inf(-1)),
    complex(0, math.NaN()),
    complex(math.NaN(), 0),
    complex(math.Inf(1), math.Inf(1)),
    complex(math.Inf(1), math.NaN()),
    complex(math.NaN(), math.Inf(1)),
    complex(math.NaN(), math.NaN())
}.slice();

internal static slice<bool> isNaNSC = new bool[]{
    false,
    false,
    false,
    true,
    true,
    false,
    false,
    false,
    true
}.slice();

// G.6.3.2

[GoType("dyn")] partial struct logSCᴛ1 {
    internal complex128 @in, want;
}
internal static slice<logSCᴛ1> logSC = new logSCᴛ1[]{
    new(complex(zero, zero),
        complex(-inf, zero)),
    new(complex(-zero, zero),
        complex(-inf, math.Pi)),
    new(complex(1.0D, inf),
        complex(inf, math.Pi / 2D)),
    new(complex(1.0D, nan),
        NaN()),
    new(complex(-inf, 1.0D),
        complex(inf, math.Pi)),
    new(complex(inf, 1.0D),
        complex(inf, 0.0D)),
    new(complex(-inf, inf),
        complex(inf, 3D * math.Pi / 4D)),
    new(complex(inf, inf),
        complex(inf, math.Pi / 4D)),
    new(complex(-inf, nan),
        complex(inf, nan)),
    new(complex(inf, nan),
        complex(inf, nan)),
    new(complex(nan, 1.0D),
        NaN()),
    new(complex(nan, inf),
        complex(inf, nan)),
    new(NaN(),
        NaN())
}.slice();

// derived from Log special cases via Log10(x) = math.Log10E*Log(x)

[GoType("dyn")] partial struct log10SCᴛ1 {
    internal complex128 @in, want;
}
internal static slice<log10SCᴛ1> log10SC = new log10SCᴛ1[]{
    new(complex(zero, zero),
        complex(-inf, zero)),
    new(complex(-zero, zero),
        complex(-inf, (float64)math.Log10E * (float64)math.Pi)),
    new(complex(1.0D, inf),
        complex(inf, (float64)math.Log10E * (float64)(math.Pi / 2D))),
    new(complex(1.0D, nan),
        NaN()),
    new(complex(-inf, 1.0D),
        complex(inf, (float64)math.Log10E * (float64)math.Pi)),
    new(complex(inf, 1.0D),
        complex(inf, 0.0D)),
    new(complex(-inf, inf),
        complex(inf, (float64)math.Log10E * (float64)(3D * math.Pi / 4D))),
    new(complex(inf, inf),
        complex(inf, (float64)math.Log10E * (float64)(math.Pi / 4D))),
    new(complex(-inf, nan),
        complex(inf, nan)),
    new(complex(inf, nan),
        complex(inf, nan)),
    new(complex(nan, 1.0D),
        NaN()),
    new(complex(nan, inf),
        complex(inf, nan)),
    new(NaN(),
        NaN())
}.slice();

internal static slice<complex128> vcPolarSC = new complex128[]{
    NaN()
}.slice();

internal static slice<ff> polarSC = new ff[]{
    new(math.NaN(), math.NaN())
}.slice();

internal static slice<array<complex128>> vcPowSC = new array<complex128>[]{
    new complex128[]{NaN(), NaN()}.array(),
    new complex128[]{0, NaN()}.array()
}.slice();

internal static slice<complex128> powSC = new complex128[]{
    NaN(),
    NaN()
}.slice();

// Derived from Sin(z) = -i * Sinh(i * z), G.6 #7

[GoType("dyn")] partial struct sinSCᴛ1 {
    internal complex128 @in, want;
}
internal static slice<sinSCᴛ1> sinSC = new sinSCᴛ1[]{
    new(complex(zero, zero),
        complex(zero, zero)),
    new(complex(zero, inf),
        complex(zero, inf)),
    new(complex(zero, nan),
        complex(zero, nan)),
    new(complex(1.0D, inf),
        complex(inf, inf)),
    new(complex(1.0D, nan),
        NaN()),
    new(complex(inf, zero),
        complex(nan, zero)),
    new(complex(inf, 1.0D),
        NaN()),
    new(complex(inf, inf),
        complex(nan, inf)),
    new(complex(inf, nan),
        NaN()),
    new(complex(nan, zero),
        complex(nan, zero)),
    new(complex(nan, 1.0D),
        NaN()),
    new(complex(nan, inf),
        complex(nan, inf)),
    new(NaN(),
        NaN())
}.slice();

// G.6.2.5
// real sign unspecified
// real sign unspecified
// +inf  cis(y)
// real sign unspecified
// real sign unspecified

[GoType("dyn")] partial struct sinhSCᴛ1 {
    internal complex128 @in, want;
}
internal static slice<sinhSCᴛ1> sinhSC = new sinhSCᴛ1[]{
    new(complex(zero, zero),
        complex(zero, zero)),
    new(complex(zero, inf),
        complex(zero, nan)),
    new(complex(zero, nan),
        complex(zero, nan)),
    new(complex(1.0D, inf),
        NaN()),
    new(complex(1.0D, nan),
        NaN()),
    new(complex(inf, zero),
        complex(inf, zero)),
    new(complex(inf, 1.0D),
        complex(inf * math.Cos(1.0D), inf * math.Sin(1.0D))),
    new(complex(inf, inf),
        complex(inf, nan)),
    new(complex(inf, nan),
        complex(inf, nan)),
    new(complex(nan, zero),
        complex(nan, zero)),
    new(complex(nan, 1.0D),
        NaN()),
    new(complex(nan, inf),
        NaN()),
    new(NaN(),
        NaN())
}.slice();

// G.6.4.2
// imaginary sign unspecified

[GoType("dyn")] partial struct sqrtSCᴛ1 {
    internal complex128 @in, want;
}
internal static slice<sqrtSCᴛ1> sqrtSC = new sqrtSCᴛ1[]{
    new(complex(zero, zero),
        complex(zero, zero)),
    new(complex(-zero, zero),
        complex(zero, zero)),
    new(complex(1.0D, inf),
        complex(inf, inf)),
    new(complex(nan, inf),
        complex(inf, inf)),
    new(complex(1.0D, nan),
        NaN()),
    new(complex(-inf, 1.0D),
        complex(zero, inf)),
    new(complex(inf, 1.0D),
        complex(inf, zero)),
    new(complex(-inf, nan),
        complex(nan, inf)),
    new(complex(inf, nan),
        complex(inf, nan)),
    new(complex(nan, 1.0D),
        NaN()),
    new(NaN(),
        NaN())
}.slice();

// Derived from Tan(z) = -i * Tanh(i * z), G.6 #7

[GoType("dyn")] partial struct tanSCᴛ1 {
    internal complex128 @in, want;
}
internal static slice<tanSCᴛ1> tanSC = new tanSCᴛ1[]{
    new(complex(zero, zero),
        complex(zero, zero)),
    new(complex(zero, nan),
        complex(zero, nan)),
    new(complex(1.0D, inf),
        complex(zero, 1.0D)),
    new(complex(1.0D, nan),
        NaN()),
    new(complex(inf, 1.0D),
        NaN()),
    new(complex(inf, inf),
        complex(zero, 1.0D)),
    new(complex(inf, nan),
        NaN()),
    new(complex(nan, zero),
        NaN()),
    new(complex(nan, 1.0D),
        NaN()),
    new(complex(nan, inf),
        complex(zero, 1.0D)),
    new(NaN(),
        NaN())
}.slice();

// G.6.2.6
// 1 + i 0 sin(2y)
// imaginary sign unspecified
// imaginary sign unspecified

[GoType("dyn")] partial struct tanhSCᴛ1 {
    internal complex128 @in, want;
}
internal static slice<tanhSCᴛ1> tanhSC = new tanhSCᴛ1[]{
    new(complex(zero, zero),
        complex(zero, zero)),
    new(complex(1.0D, inf),
        NaN()),
    new(complex(1.0D, nan),
        NaN()),
    new(complex(inf, 1.0D),
        complex(1.0D, math.Copysign(0.0D, math.Sin(2D * 1.0D)))),
    new(complex(inf, inf),
        complex(1.0D, zero)),
    new(complex(inf, nan),
        complex(1.0D, zero)),
    new(complex(nan, zero),
        complex(nan, zero)),
    new(complex(nan, 1.0D),
        NaN()),
    new(complex(nan, inf),
        NaN()),
    new(NaN(),
        NaN())
}.slice();

// branch cut continuity checks
// points on each axis at |z| > 1 are checked for one-sided continuity from both the positive and negative side
// all possible branch cuts for the elementary functions are at one of these points
internal static float64 zero = 0.0D;

internal static float64 eps = 1.0D / (9007199254740992D);

internal static slice<array<complex128>> branchPoints = new array<complex128>[]{
    new complex128[]{complex(2.0D, zero), complex(2.0D, eps)}.array(),
    new complex128[]{complex(2.0D, -zero), complex(2.0D, -eps)}.array(),
    new complex128[]{complex(-2.0D, zero), complex(-2.0D, eps)}.array(),
    new complex128[]{complex(-2.0D, -zero), complex(-2.0D, -eps)}.array(),
    new complex128[]{complex(zero, 2.0D), complex(eps, 2.0D)}.array(),
    new complex128[]{complex(-zero, 2.0D), complex(-eps, 2.0D)}.array(),
    new complex128[]{complex(zero, -2.0D), complex(eps, -2.0D)}.array(),
    new complex128[]{complex(-zero, -2.0D), complex(-eps, -2.0D)}.array()
}.slice();

// functions borrowed from pkg/math/all_test.go
internal static bool tolerance(float64 a, float64 b, float64 e) {
    var d = a - b;
    if (d < 0) {
        d = -d;
    }
    // note: b is correct (expected) value, a is actual value.
    // make error tolerance a fraction of b, not a.
    if (b != 0) {
        e = e * b;
        if (e < 0) {
            e = -e;
        }
    }
    return d < e;
}

internal static bool veryclose(float64 a, float64 b) {
    return tolerance(a, b, 4e-16D);
}

internal static bool alike(float64 a, float64 b) {
    switch (ᐧ) {
    case {} when a != a && b != b: {
        return true;
    }
    case {} when a == b: {
        return math.Signbit(a) == math.Signbit(b);
    }}

    // math.IsNaN(a) && math.IsNaN(b):
    return false;
}

internal static bool cTolerance(complex128 a, complex128 b, float64 e) {
    var d = Abs(a - b);
    if (b != 0) {
        e = e * Abs(b);
        if (e < 0) {
            e = -e;
        }
    }
    return d < e;
}

internal static bool cSoclose(complex128 a, complex128 b, float64 e) {
    return cTolerance(a, b, e);
}

internal static bool cVeryclose(complex128 a, complex128 b) {
    return cTolerance(a, b, 4e-16D);
}

internal static bool cAlike(complex128 a, complex128 b) {
    bool realAlike = default!;
    bool imagAlike = default!;
    if (isExact(real(b))){
        realAlike = alike(real(a), real(b));
    } else {
        // Allow non-exact special cases to have errors in ULP.
        realAlike = veryclose(real(a), real(b));
    }
    if (isExact(imag(b))){
        imagAlike = alike(imag(a), imag(b));
    } else {
        // Allow non-exact special cases to have errors in ULP.
        imagAlike = veryclose(imag(a), imag(b));
    }
    return realAlike && imagAlike;
}

internal static bool isExact(float64 x) {
    // Special cases that should match exactly.  Other cases are multiples
    // of Pi that may not be last bit identical on all platforms.
    return math.IsNaN(x) || math.IsInf(x, 0) || x == 0 || x == 1 || x == -1D;
}

public static void TestAbs(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vc); i++) {
        {
            var f = Abs(vc[i]); if (!veryclose(abs[i], f)) {
                Ꮡt.Errorf("Abs(%g) = %g, want %g"u8, vc[i], f, abs[i]);
            }
        }
    }
    for (nint i = 0; i < len(vcAbsSC); i++) {
        {
            var f = Abs(vcAbsSC[i]); if (!alike(absSC[i], f)) {
                Ꮡt.Errorf("Abs(%g) = %g, want %g"u8, vcAbsSC[i], f, absSC[i]);
            }
        }
    }
}

public static void TestAcos(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vc); i++) {
        {
            var f = Acos(vc[i]); if (!cSoclose(acos[i], f, 1e-14D)) {
                Ꮡt.Errorf("Acos(%g) = %g, want %g"u8, vc[i], f, acos[i]);
            }
        }
    }
    foreach (var (_, v) in acosSC) {
        {
            var f = Acos(v.@in); if (!cAlike(v.want, f)) {
                Ꮡt.Errorf("Acos(%g) = %g, want %g"u8, v.@in, f, v.want);
            }
        }
        if (math.IsNaN(imag(v.@in)) || math.IsNaN(imag(v.want))) {
            // Negating NaN is undefined with regard to the sign bit produced.
            continue;
        }
        // Acos(Conj(z))  == Conj(Acos(z))
        {
            var f = Acos(Conj(v.@in)); if (!cAlike(Conj(v.want), f) && !cAlike(v.@in, Conj(v.@in))) {
                Ꮡt.Errorf("Acos(%g) = %g, want %g"u8, Conj(v.@in), f, Conj(v.want));
            }
        }
    }
    foreach (var (_, vᴛ1) in branchPoints) {
        var pt = vᴛ1.Clone();

        {
            var (f0, f1) = (Acos(pt[0]), Acos(pt[1])); if (!cVeryclose(f0, f1)) {
                Ꮡt.Errorf("Acos(%g) not continuous, got %g want %g"u8, pt[0], f0, f1);
            }
        }
    }
}

public static void TestAcosh(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vc); i++) {
        {
            var f = Acosh(vc[i]); if (!cSoclose(acosh[i], f, 1e-14D)) {
                Ꮡt.Errorf("Acosh(%g) = %g, want %g"u8, vc[i], f, acosh[i]);
            }
        }
    }
    foreach (var (_, v) in acoshSC) {
        {
            var f = Acosh(v.@in); if (!cAlike(v.want, f)) {
                Ꮡt.Errorf("Acosh(%g) = %g, want %g"u8, v.@in, f, v.want);
            }
        }
        if (math.IsNaN(imag(v.@in)) || math.IsNaN(imag(v.want))) {
            // Negating NaN is undefined with regard to the sign bit produced.
            continue;
        }
        // Acosh(Conj(z))  == Conj(Acosh(z))
        {
            var f = Acosh(Conj(v.@in)); if (!cAlike(Conj(v.want), f) && !cAlike(v.@in, Conj(v.@in))) {
                Ꮡt.Errorf("Acosh(%g) = %g, want %g"u8, Conj(v.@in), f, Conj(v.want));
            }
        }
    }
    foreach (var (_, vᴛ1) in branchPoints) {
        var pt = vᴛ1.Clone();

        {
            var (f0, f1) = (Acosh(pt[0]), Acosh(pt[1])); if (!cVeryclose(f0, f1)) {
                Ꮡt.Errorf("Acosh(%g) not continuous, got %g want %g"u8, pt[0], f0, f1);
            }
        }
    }
}

public static void TestAsin(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vc); i++) {
        {
            var f = Asin(vc[i]); if (!cSoclose(asin[i], f, 1e-14D)) {
                Ꮡt.Errorf("Asin(%g) = %g, want %g"u8, vc[i], f, asin[i]);
            }
        }
    }
    foreach (var (_, v) in asinSC) {
        {
            var f = Asin(v.@in); if (!cAlike(v.want, f)) {
                Ꮡt.Errorf("Asin(%g) = %g, want %g"u8, v.@in, f, v.want);
            }
        }
        if (math.IsNaN(imag(v.@in)) || math.IsNaN(imag(v.want))) {
            // Negating NaN is undefined with regard to the sign bit produced.
            continue;
        }
        // Asin(Conj(z))  == Asin(Sinh(z))
        {
            var f = Asin(Conj(v.@in)); if (!cAlike(Conj(v.want), f) && !cAlike(v.@in, Conj(v.@in))) {
                Ꮡt.Errorf("Asin(%g) = %g, want %g"u8, Conj(v.@in), f, Conj(v.want));
            }
        }
        if (math.IsNaN(real(v.@in)) || math.IsNaN(real(v.want))) {
            // Negating NaN is undefined with regard to the sign bit produced.
            continue;
        }
        // Asin(-z)  == -Asin(z)
        {
            var f = Asin(-v.@in); if (!cAlike(-v.want, f) && !cAlike(v.@in, -v.@in)) {
                Ꮡt.Errorf("Asin(%g) = %g, want %g"u8, -v.@in, f, -v.want);
            }
        }
    }
    foreach (var (_, vᴛ1) in branchPoints) {
        var pt = vᴛ1.Clone();

        {
            var (f0, f1) = (Asin(pt[0]), Asin(pt[1])); if (!cVeryclose(f0, f1)) {
                Ꮡt.Errorf("Asin(%g) not continuous, got %g want %g"u8, pt[0], f0, f1);
            }
        }
    }
}

public static void TestAsinh(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vc); i++) {
        {
            var f = Asinh(vc[i]); if (!cSoclose(asinh[i], f, 4e-15D)) {
                Ꮡt.Errorf("Asinh(%g) = %g, want %g"u8, vc[i], f, asinh[i]);
            }
        }
    }
    foreach (var (_, v) in asinhSC) {
        {
            var f = Asinh(v.@in); if (!cAlike(v.want, f)) {
                Ꮡt.Errorf("Asinh(%g) = %g, want %g"u8, v.@in, f, v.want);
            }
        }
        if (math.IsNaN(imag(v.@in)) || math.IsNaN(imag(v.want))) {
            // Negating NaN is undefined with regard to the sign bit produced.
            continue;
        }
        // Asinh(Conj(z))  == Asinh(Sinh(z))
        {
            var f = Asinh(Conj(v.@in)); if (!cAlike(Conj(v.want), f) && !cAlike(v.@in, Conj(v.@in))) {
                Ꮡt.Errorf("Asinh(%g) = %g, want %g"u8, Conj(v.@in), f, Conj(v.want));
            }
        }
        if (math.IsNaN(real(v.@in)) || math.IsNaN(real(v.want))) {
            // Negating NaN is undefined with regard to the sign bit produced.
            continue;
        }
        // Asinh(-z)  == -Asinh(z)
        {
            var f = Asinh(-v.@in); if (!cAlike(-v.want, f) && !cAlike(v.@in, -v.@in)) {
                Ꮡt.Errorf("Asinh(%g) = %g, want %g"u8, -v.@in, f, -v.want);
            }
        }
    }
    foreach (var (_, vᴛ1) in branchPoints) {
        var pt = vᴛ1.Clone();

        {
            var (f0, f1) = (Asinh(pt[0]), Asinh(pt[1])); if (!cVeryclose(f0, f1)) {
                Ꮡt.Errorf("Asinh(%g) not continuous, got %g want %g"u8, pt[0], f0, f1);
            }
        }
    }
}

public static void TestAtan(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vc); i++) {
        {
            var f = Atan(vc[i]); if (!cVeryclose(atan[i], f)) {
                Ꮡt.Errorf("Atan(%g) = %g, want %g"u8, vc[i], f, atan[i]);
            }
        }
    }
    foreach (var (_, v) in atanSC) {
        {
            var f = Atan(v.@in); if (!cAlike(v.want, f)) {
                Ꮡt.Errorf("Atan(%g) = %g, want %g"u8, v.@in, f, v.want);
            }
        }
        if (math.IsNaN(imag(v.@in)) || math.IsNaN(imag(v.want))) {
            // Negating NaN is undefined with regard to the sign bit produced.
            continue;
        }
        // Atan(Conj(z))  == Conj(Atan(z))
        {
            var f = Atan(Conj(v.@in)); if (!cAlike(Conj(v.want), f) && !cAlike(v.@in, Conj(v.@in))) {
                Ꮡt.Errorf("Atan(%g) = %g, want %g"u8, Conj(v.@in), f, Conj(v.want));
            }
        }
        if (math.IsNaN(real(v.@in)) || math.IsNaN(real(v.want))) {
            // Negating NaN is undefined with regard to the sign bit produced.
            continue;
        }
        // Atan(-z)  == -Atan(z)
        {
            var f = Atan(-v.@in); if (!cAlike(-v.want, f) && !cAlike(v.@in, -v.@in)) {
                Ꮡt.Errorf("Atan(%g) = %g, want %g"u8, -v.@in, f, -v.want);
            }
        }
    }
    foreach (var (_, vᴛ1) in branchPoints) {
        var pt = vᴛ1.Clone();

        {
            var (f0, f1) = (Atan(pt[0]), Atan(pt[1])); if (!cVeryclose(f0, f1)) {
                Ꮡt.Errorf("Atan(%g) not continuous, got %g want %g"u8, pt[0], f0, f1);
            }
        }
    }
}

public static void TestAtanh(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vc); i++) {
        {
            var f = Atanh(vc[i]); if (!cVeryclose(atanh[i], f)) {
                Ꮡt.Errorf("Atanh(%g) = %g, want %g"u8, vc[i], f, atanh[i]);
            }
        }
    }
    foreach (var (_, v) in atanhSC) {
        {
            var f = Atanh(v.@in); if (!cAlike(v.want, f)) {
                Ꮡt.Errorf("Atanh(%g) = %g, want %g"u8, v.@in, f, v.want);
            }
        }
        if (math.IsNaN(imag(v.@in)) || math.IsNaN(imag(v.want))) {
            // Negating NaN is undefined with regard to the sign bit produced.
            continue;
        }
        // Atanh(Conj(z))  == Conj(Atanh(z))
        {
            var f = Atanh(Conj(v.@in)); if (!cAlike(Conj(v.want), f) && !cAlike(v.@in, Conj(v.@in))) {
                Ꮡt.Errorf("Atanh(%g) = %g, want %g"u8, Conj(v.@in), f, Conj(v.want));
            }
        }
        if (math.IsNaN(real(v.@in)) || math.IsNaN(real(v.want))) {
            // Negating NaN is undefined with regard to the sign bit produced.
            continue;
        }
        // Atanh(-z)  == -Atanh(z)
        {
            var f = Atanh(-v.@in); if (!cAlike(-v.want, f) && !cAlike(v.@in, -v.@in)) {
                Ꮡt.Errorf("Atanh(%g) = %g, want %g"u8, -v.@in, f, -v.want);
            }
        }
    }
    foreach (var (_, vᴛ1) in branchPoints) {
        var pt = vᴛ1.Clone();

        {
            var (f0, f1) = (Atanh(pt[0]), Atanh(pt[1])); if (!cVeryclose(f0, f1)) {
                Ꮡt.Errorf("Atanh(%g) not continuous, got %g want %g"u8, pt[0], f0, f1);
            }
        }
    }
}

public static void TestConj(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vc); i++) {
        {
            var f = Conj(vc[i]); if (!cVeryclose(conj[i], f)) {
                Ꮡt.Errorf("Conj(%g) = %g, want %g"u8, vc[i], f, conj[i]);
            }
        }
    }
    for (nint i = 0; i < len(vcConjSC); i++) {
        {
            var f = Conj(vcConjSC[i]); if (!cAlike(conjSC[i], f)) {
                Ꮡt.Errorf("Conj(%g) = %g, want %g"u8, vcConjSC[i], f, conjSC[i]);
            }
        }
    }
}

public static void TestCos(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vc); i++) {
        {
            var f = Cos(vc[i]); if (!cSoclose(cos[i], f, 3e-15D)) {
                Ꮡt.Errorf("Cos(%g) = %g, want %g"u8, vc[i], f, cos[i]);
            }
        }
    }
    foreach (var (_, v) in cosSC) {
        {
            var f = Cos(v.@in); if (!cAlike(v.want, f)) {
                Ꮡt.Errorf("Cos(%g) = %g, want %g"u8, v.@in, f, v.want);
            }
        }
        if (math.IsNaN(imag(v.@in)) || math.IsNaN(imag(v.want))) {
            // Negating NaN is undefined with regard to the sign bit produced.
            continue;
        }
        // Cos(Conj(z))  == Cos(Cosh(z))
        {
            var f = Cos(Conj(v.@in)); if (!cAlike(Conj(v.want), f) && !cAlike(v.@in, Conj(v.@in))) {
                Ꮡt.Errorf("Cos(%g) = %g, want %g"u8, Conj(v.@in), f, Conj(v.want));
            }
        }
        if (math.IsNaN(real(v.@in)) || math.IsNaN(real(v.want))) {
            // Negating NaN is undefined with regard to the sign bit produced.
            continue;
        }
        // Cos(-z)  == Cos(z)
        {
            var f = Cos(-v.@in); if (!cAlike(v.want, f) && !cAlike(v.@in, -v.@in)) {
                Ꮡt.Errorf("Cos(%g) = %g, want %g"u8, -v.@in, f, v.want);
            }
        }
    }
}

public static void TestCosh(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vc); i++) {
        {
            var f = Cosh(vc[i]); if (!cSoclose(cosh[i], f, 2e-15D)) {
                Ꮡt.Errorf("Cosh(%g) = %g, want %g"u8, vc[i], f, cosh[i]);
            }
        }
    }
    foreach (var (_, v) in coshSC) {
        {
            var f = Cosh(v.@in); if (!cAlike(v.want, f)) {
                Ꮡt.Errorf("Cosh(%g) = %g, want %g"u8, v.@in, f, v.want);
            }
        }
        if (math.IsNaN(imag(v.@in)) || math.IsNaN(imag(v.want))) {
            // Negating NaN is undefined with regard to the sign bit produced.
            continue;
        }
        // Cosh(Conj(z))  == Conj(Cosh(z))
        {
            var f = Cosh(Conj(v.@in)); if (!cAlike(Conj(v.want), f) && !cAlike(v.@in, Conj(v.@in))) {
                Ꮡt.Errorf("Cosh(%g) = %g, want %g"u8, Conj(v.@in), f, Conj(v.want));
            }
        }
        if (math.IsNaN(real(v.@in)) || math.IsNaN(real(v.want))) {
            // Negating NaN is undefined with regard to the sign bit produced.
            continue;
        }
        // Cosh(-z)  == Cosh(z)
        {
            var f = Cosh(-v.@in); if (!cAlike(v.want, f) && !cAlike(v.@in, -v.@in)) {
                Ꮡt.Errorf("Cosh(%g) = %g, want %g"u8, -v.@in, f, v.want);
            }
        }
    }
}

public static void TestExp(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vc); i++) {
        {
            var f = Exp(vc[i]); if (!cSoclose(exp[i], f, 1e-15D)) {
                Ꮡt.Errorf("Exp(%g) = %g, want %g"u8, vc[i], f, exp[i]);
            }
        }
    }
    foreach (var (_, v) in expSC) {
        {
            var f = Exp(v.@in); if (!cAlike(v.want, f)) {
                Ꮡt.Errorf("Exp(%g) = %g, want %g"u8, v.@in, f, v.want);
            }
        }
        if (math.IsNaN(imag(v.@in)) || math.IsNaN(imag(v.want))) {
            // Negating NaN is undefined with regard to the sign bit produced.
            continue;
        }
        // Exp(Conj(z))  == Exp(Cosh(z))
        {
            var f = Exp(Conj(v.@in)); if (!cAlike(Conj(v.want), f) && !cAlike(v.@in, Conj(v.@in))) {
                Ꮡt.Errorf("Exp(%g) = %g, want %g"u8, Conj(v.@in), f, Conj(v.want));
            }
        }
    }
}

public static void TestIsNaN(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vcIsNaNSC); i++) {
        {
            var f = IsNaN(vcIsNaNSC[i]); if (isNaNSC[i] != f) {
                Ꮡt.Errorf("IsNaN(%v) = %v, want %v"u8, vcIsNaNSC[i], f, isNaNSC[i]);
            }
        }
    }
}

public static void TestLog(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vc); i++) {
        {
            var f = Log(vc[i]); if (!cVeryclose(log[i], f)) {
                Ꮡt.Errorf("Log(%g) = %g, want %g"u8, vc[i], f, log[i]);
            }
        }
    }
    foreach (var (_, v) in logSC) {
        {
            var f = Log(v.@in); if (!cAlike(v.want, f)) {
                Ꮡt.Errorf("Log(%g) = %g, want %g"u8, v.@in, f, v.want);
            }
        }
        if (math.IsNaN(imag(v.@in)) || math.IsNaN(imag(v.want))) {
            // Negating NaN is undefined with regard to the sign bit produced.
            continue;
        }
        // Log(Conj(z))  == Conj(Log(z))
        {
            var f = Log(Conj(v.@in)); if (!cAlike(Conj(v.want), f) && !cAlike(v.@in, Conj(v.@in))) {
                Ꮡt.Errorf("Log(%g) = %g, want %g"u8, Conj(v.@in), f, Conj(v.want));
            }
        }
    }
    foreach (var (_, vᴛ1) in branchPoints) {
        var pt = vᴛ1.Clone();

        {
            var (f0, f1) = (Log(pt[0]), Log(pt[1])); if (!cVeryclose(f0, f1)) {
                Ꮡt.Errorf("Log(%g) not continuous, got %g want %g"u8, pt[0], f0, f1);
            }
        }
    }
}

public static void TestLog10(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vc); i++) {
        {
            var f = Log10(vc[i]); if (!cVeryclose(log10[i], f)) {
                Ꮡt.Errorf("Log10(%g) = %g, want %g"u8, vc[i], f, log10[i]);
            }
        }
    }
    foreach (var (_, v) in log10SC) {
        {
            var f = Log10(v.@in); if (!cAlike(v.want, f)) {
                Ꮡt.Errorf("Log10(%g) = %g, want %g"u8, v.@in, f, v.want);
            }
        }
        if (math.IsNaN(imag(v.@in)) || math.IsNaN(imag(v.want))) {
            // Negating NaN is undefined with regard to the sign bit produced.
            continue;
        }
        // Log10(Conj(z))  == Conj(Log10(z))
        {
            var f = Log10(Conj(v.@in)); if (!cAlike(Conj(v.want), f) && !cAlike(v.@in, Conj(v.@in))) {
                Ꮡt.Errorf("Log10(%g) = %g, want %g"u8, Conj(v.@in), f, Conj(v.want));
            }
        }
    }
}

public static void TestPolar(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vc); i++) {
        {
            var (r, theta) = Polar(vc[i]); if (!veryclose(polar[i].r, r) && !veryclose(polar[i].theta, theta)) {
                Ꮡt.Errorf("Polar(%g) = %g, %g want %g, %g"u8, vc[i], r, theta, polar[i].r, polar[i].theta);
            }
        }
    }
    for (nint i = 0; i < len(vcPolarSC); i++) {
        {
            var (r, theta) = Polar(vcPolarSC[i]); if (!alike(polarSC[i].r, r) && !alike(polarSC[i].theta, theta)) {
                Ꮡt.Errorf("Polar(%g) = %g, %g, want %g, %g"u8, vcPolarSC[i], r, theta, polarSC[i].r, polarSC[i].theta);
            }
        }
    }
}

public static void TestPow(ж<testing.T> Ꮡt) {
    // Special cases for Pow(0, c).
    complex128 zero = complex(0D, 0D);
    var zeroPowers = new array<complex128>[]{
        new complex128[]{0, 1D + 0D.i()}.array(),
        new complex128[]{1.5D, 0D + 0D.i()}.array(),
        new complex128[]{-1.5D, complex(math.Inf(0), 0)}.array(),
        new complex128[]{-1.5D + 1.5D.i(), Inf()}.array()
    }.slice();
    foreach (var (_, vᴛ1) in zeroPowers) {
        var zp = vᴛ1.Clone();

        {
            var f = Pow(zero, zp[0]); if (f != zp[1]) {
                Ꮡt.Errorf("Pow(%g, %g) = %g, want %g"u8, zero, zp[0], f, zp[1]);
            }
        }
    }
    complex128 a = complex(3.0D, 3.0D);
    for (nint i = 0; i < len(vc); i++) {
        {
            var f = Pow(a, vc[i]); if (!cSoclose(pow[i], f, 4e-15D)) {
                Ꮡt.Errorf("Pow(%g, %g) = %g, want %g"u8, a, vc[i], f, pow[i]);
            }
        }
    }
    for (nint i = 0; i < len(vcPowSC); i++) {
        {
            var f = Pow(vcPowSC[i][0], vcPowSC[i][1]); if (!cAlike(powSC[i], f)) {
                Ꮡt.Errorf("Pow(%g, %g) = %g, want %g"u8, vcPowSC[i][0], vcPowSC[i][1], f, powSC[i]);
            }
        }
    }
    foreach (var (_, vᴛ2) in branchPoints) {
        var pt = vᴛ2.Clone();

        {
            var (f0, f1) = (Pow(pt[0], 0.1D), Pow(pt[1], 0.1D)); if (!cVeryclose(f0, f1)) {
                Ꮡt.Errorf("Pow(%g, 0.1) not continuous, got %g want %g"u8, pt[0], f0, f1);
            }
        }
    }
}

public static void TestRect(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vc); i++) {
        {
            var f = Rect(polar[i].r, polar[i].theta); if (!cVeryclose(vc[i], f)) {
                Ꮡt.Errorf("Rect(%g, %g) = %g want %g"u8, polar[i].r, polar[i].theta, f, vc[i]);
            }
        }
    }
    for (nint i = 0; i < len(vcPolarSC); i++) {
        {
            var f = Rect(polarSC[i].r, polarSC[i].theta); if (!cAlike(vcPolarSC[i], f)) {
                Ꮡt.Errorf("Rect(%g, %g) = %g, want %g"u8, polarSC[i].r, polarSC[i].theta, f, vcPolarSC[i]);
            }
        }
    }
}

public static void TestSin(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vc); i++) {
        {
            var f = Sin(vc[i]); if (!cSoclose(sin[i], f, 2e-15D)) {
                Ꮡt.Errorf("Sin(%g) = %g, want %g"u8, vc[i], f, sin[i]);
            }
        }
    }
    foreach (var (_, v) in sinSC) {
        {
            var f = Sin(v.@in); if (!cAlike(v.want, f)) {
                Ꮡt.Errorf("Sin(%g) = %g, want %g"u8, v.@in, f, v.want);
            }
        }
        if (math.IsNaN(imag(v.@in)) || math.IsNaN(imag(v.want))) {
            // Negating NaN is undefined with regard to the sign bit produced.
            continue;
        }
        // Sin(Conj(z))  == Conj(Sin(z))
        {
            var f = Sin(Conj(v.@in)); if (!cAlike(Conj(v.want), f) && !cAlike(v.@in, Conj(v.@in))) {
                Ꮡt.Errorf("Sinh(%g) = %g, want %g"u8, Conj(v.@in), f, Conj(v.want));
            }
        }
        if (math.IsNaN(real(v.@in)) || math.IsNaN(real(v.want))) {
            // Negating NaN is undefined with regard to the sign bit produced.
            continue;
        }
        // Sin(-z)  == -Sin(z)
        {
            var f = Sin(-v.@in); if (!cAlike(-v.want, f) && !cAlike(v.@in, -v.@in)) {
                Ꮡt.Errorf("Sinh(%g) = %g, want %g"u8, -v.@in, f, -v.want);
            }
        }
    }
}

public static void TestSinh(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vc); i++) {
        {
            var f = Sinh(vc[i]); if (!cSoclose(sinh[i], f, 2e-15D)) {
                Ꮡt.Errorf("Sinh(%g) = %g, want %g"u8, vc[i], f, sinh[i]);
            }
        }
    }
    foreach (var (_, v) in sinhSC) {
        {
            var f = Sinh(v.@in); if (!cAlike(v.want, f)) {
                Ꮡt.Errorf("Sinh(%g) = %g, want %g"u8, v.@in, f, v.want);
            }
        }
        if (math.IsNaN(imag(v.@in)) || math.IsNaN(imag(v.want))) {
            // Negating NaN is undefined with regard to the sign bit produced.
            continue;
        }
        // Sinh(Conj(z))  == Conj(Sinh(z))
        {
            var f = Sinh(Conj(v.@in)); if (!cAlike(Conj(v.want), f) && !cAlike(v.@in, Conj(v.@in))) {
                Ꮡt.Errorf("Sinh(%g) = %g, want %g"u8, Conj(v.@in), f, Conj(v.want));
            }
        }
        if (math.IsNaN(real(v.@in)) || math.IsNaN(real(v.want))) {
            // Negating NaN is undefined with regard to the sign bit produced.
            continue;
        }
        // Sinh(-z)  == -Sinh(z)
        {
            var f = Sinh(-v.@in); if (!cAlike(-v.want, f) && !cAlike(v.@in, -v.@in)) {
                Ꮡt.Errorf("Sinh(%g) = %g, want %g"u8, -v.@in, f, -v.want);
            }
        }
    }
}

public static void TestSqrt(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vc); i++) {
        {
            var f = Sqrt(vc[i]); if (!cVeryclose(sqrt[i], f)) {
                Ꮡt.Errorf("Sqrt(%g) = %g, want %g"u8, vc[i], f, sqrt[i]);
            }
        }
    }
    foreach (var (_, v) in sqrtSC) {
        {
            var f = Sqrt(v.@in); if (!cAlike(v.want, f)) {
                Ꮡt.Errorf("Sqrt(%g) = %g, want %g"u8, v.@in, f, v.want);
            }
        }
        if (math.IsNaN(imag(v.@in)) || math.IsNaN(imag(v.want))) {
            // Negating NaN is undefined with regard to the sign bit produced.
            continue;
        }
        // Sqrt(Conj(z)) == Conj(Sqrt(z))
        {
            var f = Sqrt(Conj(v.@in)); if (!cAlike(Conj(v.want), f) && !cAlike(v.@in, Conj(v.@in))) {
                Ꮡt.Errorf("Sqrt(%g) = %g, want %g"u8, Conj(v.@in), f, Conj(v.want));
            }
        }
    }
    foreach (var (_, vᴛ1) in branchPoints) {
        var pt = vᴛ1.Clone();

        {
            var (f0, f1) = (Sqrt(pt[0]), Sqrt(pt[1])); if (!cVeryclose(f0, f1)) {
                Ꮡt.Errorf("Sqrt(%g) not continuous, got %g want %g"u8, pt[0], f0, f1);
            }
        }
    }
}

public static void TestTan(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vc); i++) {
        {
            var f = Tan(vc[i]); if (!cSoclose(tan[i], f, 3e-15D)) {
                Ꮡt.Errorf("Tan(%g) = %g, want %g"u8, vc[i], f, tan[i]);
            }
        }
    }
    foreach (var (_, v) in tanSC) {
        {
            var f = Tan(v.@in); if (!cAlike(v.want, f)) {
                Ꮡt.Errorf("Tan(%g) = %g, want %g"u8, v.@in, f, v.want);
            }
        }
        if (math.IsNaN(imag(v.@in)) || math.IsNaN(imag(v.want))) {
            // Negating NaN is undefined with regard to the sign bit produced.
            continue;
        }
        // Tan(Conj(z))  == Conj(Tan(z))
        {
            var f = Tan(Conj(v.@in)); if (!cAlike(Conj(v.want), f) && !cAlike(v.@in, Conj(v.@in))) {
                Ꮡt.Errorf("Tan(%g) = %g, want %g"u8, Conj(v.@in), f, Conj(v.want));
            }
        }
        if (math.IsNaN(real(v.@in)) || math.IsNaN(real(v.want))) {
            // Negating NaN is undefined with regard to the sign bit produced.
            continue;
        }
        // Tan(-z)  == -Tan(z)
        {
            var f = Tan(-v.@in); if (!cAlike(-v.want, f) && !cAlike(v.@in, -v.@in)) {
                Ꮡt.Errorf("Tan(%g) = %g, want %g"u8, -v.@in, f, -v.want);
            }
        }
    }
}

public static void TestTanh(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vc); i++) {
        {
            var f = Tanh(vc[i]); if (!cSoclose(tanh[i], f, 2e-15D)) {
                Ꮡt.Errorf("Tanh(%g) = %g, want %g"u8, vc[i], f, tanh[i]);
            }
        }
    }
    foreach (var (_, v) in tanhSC) {
        {
            var f = Tanh(v.@in); if (!cAlike(v.want, f)) {
                Ꮡt.Errorf("Tanh(%g) = %g, want %g"u8, v.@in, f, v.want);
            }
        }
        if (math.IsNaN(imag(v.@in)) || math.IsNaN(imag(v.want))) {
            // Negating NaN is undefined with regard to the sign bit produced.
            continue;
        }
        // Tanh(Conj(z))  == Conj(Tanh(z))
        {
            var f = Tanh(Conj(v.@in)); if (!cAlike(Conj(v.want), f) && !cAlike(v.@in, Conj(v.@in))) {
                Ꮡt.Errorf("Tanh(%g) = %g, want %g"u8, Conj(v.@in), f, Conj(v.want));
            }
        }
        if (math.IsNaN(real(v.@in)) || math.IsNaN(real(v.want))) {
            // Negating NaN is undefined with regard to the sign bit produced.
            continue;
        }
        // Tanh(-z)  == -Tanh(z)
        {
            var f = Tanh(-v.@in); if (!cAlike(-v.want, f) && !cAlike(v.@in, -v.@in)) {
                Ꮡt.Errorf("Tanh(%g) = %g, want %g"u8, -v.@in, f, -v.want);
            }
        }
    }
}

// See issue 17577
public static void TestInfiniteLoopIntanSeries(ж<testing.T> Ꮡt) {
    var want = Inf();
    {
        var got = Cot(0); if (got != want) {
            Ꮡt.Errorf("Cot(0): got %g, want %g"u8, got, want);
        }
    }
}

public static void BenchmarkAbs(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.Value;

    for (nint i = 0; i < b.N; i++) {
        Abs(complex(2.5D, 3.5D));
    }
}

public static void BenchmarkAcos(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.Value;

    for (nint i = 0; i < b.N; i++) {
        Acos(complex(2.5D, 3.5D));
    }
}

public static void BenchmarkAcosh(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.Value;

    for (nint i = 0; i < b.N; i++) {
        Acosh(complex(2.5D, 3.5D));
    }
}

public static void BenchmarkAsin(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.Value;

    for (nint i = 0; i < b.N; i++) {
        Asin(complex(2.5D, 3.5D));
    }
}

public static void BenchmarkAsinh(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.Value;

    for (nint i = 0; i < b.N; i++) {
        Asinh(complex(2.5D, 3.5D));
    }
}

public static void BenchmarkAtan(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.Value;

    for (nint i = 0; i < b.N; i++) {
        Atan(complex(2.5D, 3.5D));
    }
}

public static void BenchmarkAtanh(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.Value;

    for (nint i = 0; i < b.N; i++) {
        Atanh(complex(2.5D, 3.5D));
    }
}

public static void BenchmarkConj(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.Value;

    for (nint i = 0; i < b.N; i++) {
        Conj(complex(2.5D, 3.5D));
    }
}

public static void BenchmarkCos(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.Value;

    for (nint i = 0; i < b.N; i++) {
        Cos(complex(2.5D, 3.5D));
    }
}

public static void BenchmarkCosh(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.Value;

    for (nint i = 0; i < b.N; i++) {
        Cosh(complex(2.5D, 3.5D));
    }
}

public static void BenchmarkExp(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.Value;

    for (nint i = 0; i < b.N; i++) {
        Exp(complex(2.5D, 3.5D));
    }
}

public static void BenchmarkLog(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.Value;

    for (nint i = 0; i < b.N; i++) {
        Log(complex(2.5D, 3.5D));
    }
}

public static void BenchmarkLog10(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.Value;

    for (nint i = 0; i < b.N; i++) {
        Log10(complex(2.5D, 3.5D));
    }
}

public static void BenchmarkPhase(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.Value;

    for (nint i = 0; i < b.N; i++) {
        Phase(complex(2.5D, 3.5D));
    }
}

public static void BenchmarkPolar(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.Value;

    for (nint i = 0; i < b.N; i++) {
        Polar(complex(2.5D, 3.5D));
    }
}

public static void BenchmarkPow(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.Value;

    for (nint i = 0; i < b.N; i++) {
        Pow(complex(2.5D, 3.5D), complex(2.5D, 3.5D));
    }
}

public static void BenchmarkRect(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.Value;

    for (nint i = 0; i < b.N; i++) {
        Rect(2.5D, 1.5D);
    }
}

public static void BenchmarkSin(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.Value;

    for (nint i = 0; i < b.N; i++) {
        Sin(complex(2.5D, 3.5D));
    }
}

public static void BenchmarkSinh(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.Value;

    for (nint i = 0; i < b.N; i++) {
        Sinh(complex(2.5D, 3.5D));
    }
}

public static void BenchmarkSqrt(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.Value;

    for (nint i = 0; i < b.N; i++) {
        Sqrt(complex(2.5D, 3.5D));
    }
}

public static void BenchmarkTan(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.Value;

    for (nint i = 0; i < b.N; i++) {
        Tan(complex(2.5D, 3.5D));
    }
}

public static void BenchmarkTanh(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.Value;

    for (nint i = 0; i < b.N; i++) {
        Tanh(complex(2.5D, 3.5D));
    }
}

} // end cmplx_package
