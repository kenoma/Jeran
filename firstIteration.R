library(RHRV)
#Неплохой туториал по этой библиотеке можно найти тут
#http://rhrv.r-forge.r-project.org/tutorial/tutorial.pdf

dir<-"D:/projects/Jeran/Processing";

colClasses = c("character", "numeric", "numeric", "numeric", "numeric", "numeric", "numeric", "numeric", "numeric", "numeric", "numeric", "numeric", "numeric", "numeric", "numeric", "numeric", "numeric", "numeric")
col.names = c("File","SDNN","SDANN","SDNNIDX","pNN50","SDSD","rMSSD","IRRR","MADRR","TINN","HRVindex","ULF","VLF","LF","HF","HRV","LFHF", "EmbeddingDim")

df <- read.table(text = "",
                 colClasses = colClasses,
                 col.names = col.names)
line = 0;
for(fn in list.files(dir,pattern="*.bin"))
{
#  fn<-list.files(dir,pattern="*.bin")[1]
  y<-read.csv(paste(dir, fn, sep="/"))
  y<-y[y$Dilemma>0,]
  cy <-cumsum(y$RR/1000.0)
  y$T<-cy
  write(cy,paste(dir, "cumsum/tmp", sep="/"),sep = "\r")
  
  #обрабатываем данные с помощью библиотеки RHRV - фильтруем выбросы, обсчитываем основные показатели
  hrv.data = CreateHRVData()
  hrv.data = SetVerbose(hrv.data, TRUE )
  hrv.data = LoadBeatAscii(hrv.data, paste(dir, "cumsum/tmp", sep="/"), RecordPath = ".")
  hrv.data = BuildNIHR(hrv.data)
  hrv.data = FilterNIHR(hrv.data)
  hrv.data = EditNIHR(hrv.data)
  
  hrv.data = AddEpisodes(hrv.data, 
                         InitTimes = aggregate(y$T, by=list(y$Dilemma), FUN=min)[,2], 
                         Tags = paste("D", aggregate(y$T, by=list(y$Dilemma), FUN=any)[,1], sep="_"), 
                         Durations = aggregate(y$T, by=list(y$Dilemma), FUN=max)[,2]-aggregate(y$T, by=list(y$Dilemma), FUN=min)[,2], 
                         Values = list(rep(0, 32)))
  PlotNIHR(hrv.data, Tag=paste("D", seq(1,32), sep="_"))
  hrv.data = InterpolateNIHR (hrv.data, freqhr = 4)
  hrv.data = CreateTimeAnalysis(hrv.data, size = 200, interval = 8)
  hrv.data = CreateFreqAnalysis(hrv.data)
  hrv.data = CalculatePowerBand( hrv.data , 
                                 indexFreqAnalysis= 1, 
                                 type = "wavelet", 
                                 wavelet = "la8", 
                                 bandtolerance = 0.01, 
                                 relative = FALSE, 
                                 ULFmin = 0, 
                                 ULFmax = 0.03, 
                                 VLFmin = 0.03, 
                                 VLFmax = 0.05, 
                                 LFmin = 0.05, 
                                 LFmax = 0.15, 
                                 HFmin = 0.15, 
                                 HFmax = 0.4 )
  hrv.data = CreateNonLinearAnalysis(hrv.data)
  
  line <-line+1;
  df[line,"File"]    <-fn
  df[line,"SDNN"]  	<-hrv.data$TimeAnalysis[[1]]$SDNN
  df[line,"SDANN"]		<-hrv.data$TimeAnalysis[[1]]$SDANN
  df[line,"SDNNIDX"]	<-hrv.data$TimeAnalysis[[1]]$SDNNIDX
  df[line,"pNN50"]		<-hrv.data$TimeAnalysis[[1]]$pNN50
  df[line,"SDSD"]		<-hrv.data$TimeAnalysis[[1]]$SDSD
  df[line,"rMSSD"]		<-hrv.data$TimeAnalysis[[1]]$rMSSD
  df[line,"IRRR"]		<-hrv.data$TimeAnalysis[[1]]$IRRR
  df[line,"MADRR"]		<-hrv.data$TimeAnalysis[[1]]$MADRR
  df[line,"TINN"]		<-hrv.data$TimeAnalysis[[1]]$TINN
  #df[line,"HRVindex"]	<-hrv.data$TimeAnalysis[[1]]$HRVindex
  cat("ts")
  df[line,"ULF"]	<-	mean(hrv.data$FreqAnalysis[[1]]$ULF)
  df[line,"VLF"]	<-	mean(hrv.data$FreqAnalysis[[1]]$VLF)
  df[line,"LF"]		<-	mean(hrv.data$FreqAnalysis[[1]]$LF)
  df[line,"HF"]		<-	mean(hrv.data$FreqAnalysis[[1]]$HF)
  df[line,"HRV"]	<-	mean(hrv.data$FreqAnalysis[[1]]$HRV)
  df[line,"LFHF"]	<-	mean(hrv.data$FreqAnalysis[[1]]$LFHF)
  cat("fq")
  df[line,"EmbeddingDim"]<-CalculateEmbeddingDim(hrv.data, numberPoints = 10000, timeLag = kTimeLag, maxEmbeddingDim = 15)
}

write.csv(df, file = paste(dir, "cumsum/stats.csv", sep="/"))

