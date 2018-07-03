
library(ggplot2)
library(reshape2)
# install.packages('gridExtra')
library(gridExtra)
# install.packages('triangle')
library(triangle)

(WD <- getwd())

cost <- read.csv("C:\\Users\\kollaea\\Documents\\Ed's Library\\Assignments\\Budget Sim GUI\\MonteCarloSimCost.csv",stringsAsFactors=F)

TargetBudget <- sum(cost$TARGET)
simNumber <- 10000


results <- 1:simNumber
for(i in 1:nrow(cost)){
  x <- cost[i,]
  range <- x$LOW:x$HIGH
  
  simCosts <- if(length(range)==1){
    rep(range,simNumber)
  } else {
    probs <- c(rep(x$PROB/which(range==x$TARGET),which(range==x$TARGET)),
               rep((100-x$PROB)/(length(range)-which(range==x$TARGET)),length(range)-which(range==x$TARGET)))
    sample(x=range,size=simNumber,prob=probs/100,replace=T)
  }
  
  results <- cbind(results,simCosts)
}

results <- results[,-1]
results <- data.frame(results)
colnames(results) <- cost$ELEMENT


SimBudgetTotals <- data.frame(Cost=rowSums(results))

hist <- ggplot(SimBudgetTotals,aes(x=Cost))+
  geom_histogram(color='black',fill='blue')+
  geom_vline(xintercept=TargetBudget,color='red') +
  geom_text(aes(label='Target',x=TargetBudget,y=0,vjust=1),colour='red') +
  geom_vline(xintercept=quantile(SimBudgetTotals$Cost,.95),color='green') +
  geom_text(aes(label='95%',x=quantile(SimBudgetTotals$Cost,.95),y=0,vjust=1),colour='green') +
  labs(title = paste(as.integer(simNumber),'Simulations of Project Budget'),
       subtitle = paste(round(100*nrow(results[rowSums(results) <= TargetBudget,])/nrow(results),2),'% Chance of Staying on Budget','\n',round(quantile(SimBudgetTotals$Cost,.95)-TargetBudget,2),' Contingency Needed for 95% Probability of Staying on Budget',sep=''))


budgetChange <- data.frame(melt(round(sapply(results[rowSums(results) >= quantile(SimBudgetTotals$Cost,.95) & rowSums(results) <= quantile(SimBudgetTotals$Cost,.96),],mean) - cost$TARGET)))
budgetChange$variable <- rownames(budgetChange)
budgetChange$color <- 'red'
budgetChange$color[budgetChange$value < 0] <- 'green'


bar <- ggplot(budgetChange,aes(x=variable,y=value,fill=color))+
  geom_bar(stat='identity')+
  coord_flip()+
  scale_fill_identity() + 
  geom_text(aes(label = value),hjust=1) +
  labs(title = paste('Allocation of Needed Contigency'))

#Export Results to PDF
#pdf("C:\\Users\\kollaea\\Source\\Repos\\Budget Simulator\\Budget Simulator\\Resources\\budget.pdf",width=11,height=8)
#Export Results to PNG
png("C:\\Users\\kollaea\\Source\\Repos\\Budget Simulator\\Budget Simulator\\Resources\\budget.png",width=900,height=660)
grid.arrange(hist,bar,ncol=2)
dev.off()

